using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Payment;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System.Security;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClaimService _service;
        private readonly AppSettings _appSettings;
        private readonly IEnrollmentService _enrollmentService;

        public PaymentService(IUnitOfWork uow, IClaimService service, AppSettings appSettings, IEnrollmentService enrollmentService)
        {
            _uow = uow;
            _service = service;
            _appSettings = appSettings;
            _enrollmentService = enrollmentService;
        }

        public async Task<ApiResponse> GetSuccessfulPaymentRecordsAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var paymentList = await _uow.Payments.GetAllAsync(p => p.Status == 1 && p.PaidAt != null);
                var result = paymentList.Select(p => new PaymentRecord
                {
                    PaidAt = p.PaidAt,
                    Amount = p.Amount
                }).ToList();
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<OnlineLearningPlatform.BusinessObject.Responses.Admin.TopCourseResponse> GetTopCourseByEnrollmentsAsync()
        {
            // group enrollments by course and pick top
            var enrollments = await _uow.Enrollments.GetAllAsync(e => true);
            var grouped = enrollments.GroupBy(e => e.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            if (grouped == null) return null!;

            var course = await _uow.Courses.GetAsync(c => c.CourseId == grouped.CourseId);
            return new OnlineLearningPlatform.BusinessObject.Responses.Admin.TopCourseResponse
            {
                CourseId = grouped.CourseId,
                Title = course?.Title ?? "",
                EnrollCount = grouped.Count
            };
        }

        public async Task<OnlineLearningPlatform.BusinessObject.Responses.Admin.TopInstructorResponse> GetTopInstructorByStudentsAsync()
        {
            // Count students per instructor via enrollments -> course.CreatedBy
            var enrollments = await _uow.Enrollments.GetAllAsync(e => true);
            var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
            var courses = await _uow.Courses.GetAllAsync(c => courseIds.Contains(c.CourseId));

            var instructorCounts = new Dictionary<Guid, int>();
            foreach (var e in enrollments)
            {
                var c = courses.FirstOrDefault(x => x.CourseId == e.CourseId);
                if (c == null) continue;
                if (!instructorCounts.ContainsKey(c.CreatedBy)) instructorCounts[c.CreatedBy] = 0;
                instructorCounts[c.CreatedBy]++;
            }

            if (!instructorCounts.Any()) return null!;

            var top = instructorCounts.OrderByDescending(kv => kv.Value).First();
            var instructor = await _uow.Users.GetAsync(u => u.UserId == top.Key);

            return new OnlineLearningPlatform.BusinessObject.Responses.Admin.TopInstructorResponse
            {
                InstructorId = top.Key,
                InstructorName = instructor?.FullName ?? instructor?.Email ?? "",
                StudentCount = top.Value
            };
        }
        public async Task<PaymentResponse> CreatePayOSPaymentAsync(CreateNewPaymentRequest request)
        {
            var userId = _service.GetUserClaim().UserId;
            var course = await _uow.Courses.GetAsync(c => c.CourseId == request.CourseId) ?? throw new NotFoundException("Course not found");

            // Always use server-side price from DB — never trust client-submitted amount
            var coursePrice = course.Price;

            var alreadyEnrolled = await _uow.Enrollments.AnyAsync(e =>
                e.UserId == userId &&
                e.CourseId == request.CourseId &&
                e.Status == 1);

            if (alreadyEnrolled)
                throw new Exception("User already enrolled in this course");

            var activePending = await _uow.Payments.GetAsync(p =>
               p.UserId == userId &&
               p.CourseId == request.CourseId &&
               p.Status == 0 &&
               p.ExpiredAt > DateTime.UtcNow);

            if (activePending != null)
            {
                return new PaymentResponse
                {
                    CheckoutUrl = activePending.CheckoutUrl
                };
            }

            var payOS = new PayOSClient(
                _appSettings.PayOS.ClientId,
                _appSettings.PayOS.ApiKey,
                _appSettings.PayOS.ChecksumKey
            );

            var expireMinutes = 15;
            var requestData = new CreatePaymentLinkRequest
            {
                OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Amount = (int)coursePrice,
                Description = $"Course: {course.Title}",
                ReturnUrl = _appSettings.PayOS.ReturnUrl,
                CancelUrl = _appSettings.PayOS.CancelUrl,
                ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(expireMinutes).ToUnixTimeSeconds()
            };

            var response = await payOS.PaymentRequests.CreateAsync(requestData);
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                UserId = userId,
                CourseId = course.CourseId,
                Amount = coursePrice,
                Status = 0,
                OrderCode = requestData.OrderCode,
                PaymentLinkId = response.PaymentLinkId,
                CheckoutUrl = response.CheckoutUrl,
                Currency = "VND",
                Method = "PayOS",
                Type = 0,
                ExpiredAt = DateTime.UtcNow.AddMinutes(expireMinutes)
            };

            await _uow.Payments.AddAsync(payment);
            await _uow.SaveChangeAsync();
            return new PaymentResponse
            {
                CheckoutUrl = response.CheckoutUrl
            };
        }

        public async Task HandlePayOSWebhookAsync(WebhookData data)
        {
            await _uow.BeginTransactionAsync();

            try
            {
                var payment = await _uow.Payments
                    .GetAsync(p => p.OrderCode == data.OrderCode)
                    ?? throw new Exception("Payment not found");

                // 🔐 Anti fake
                if (payment.PaymentLinkId != data.PaymentLinkId)
                    throw new SecurityException("Invalid PaymentLinkId");

                if (payment.Amount != data.Amount)
                    throw new SecurityException("Amount mismatch");

                // 🔁 Idempotent
                if (payment.Status == 1)
                {
                    await _uow.RollbackAsync();
                    return;
                }

                // ❌ Thanh toán thất bại
                if (data.Code != "00")
                {
                    payment.Status = 2;
                    _uow.Payments.Update(payment);

                    await _uow.CommitAsync();
                    return;
                }

                // ✅ Thanh toán thành công
                payment.Status = 1;
                payment.PaidAt = DateTime.UtcNow;
                payment.Reference = data.Reference;
                payment.CounterAccountNumber = data.CounterAccountNumber;
                payment.CounterAccountName = data.CounterAccountName;
                payment.CounterAccountBankName = data.CounterAccountBankName;

                // 🎓 Enrollment (chống trùng)
                var existedEnrollment = await _uow.Enrollments.AnyAsync(e =>
                    e.UserId == payment.UserId &&
                    e.CourseId == payment.CourseId);

                if (!existedEnrollment)
                {
                    var enrollment = new Enrollment
                    {
                        EnrollmentId = Guid.NewGuid(),
                        UserId = payment.UserId,
                        CourseId = payment.CourseId!.Value,
                        ProgressPercent = 0,
                        Status = 1,
                        EnrolledAt = DateTime.UtcNow
                    };

                    await _uow.Enrollments.AddAsync(enrollment);

                    // Initialize lesson progress records so MarkLessonCompleted works
                    await _enrollmentService.InitializeLessonProgressAsync(payment.UserId, payment.CourseId!.Value);
                }

                // 💰 Wallet
                var course = await _uow.Courses.GetAsync(c => c.CourseId == payment.CourseId)
                    ?? throw new Exception("Course not found");

                var instructorAmount = course.Price * 0.8m;

                var existedWalletTxn = await _uow.WalletTransactions.AnyAsync(t =>
                    t.PaymentId == payment.PaymentId &&
                    t.TransactionType == 0);

                if (!existedWalletTxn)
                {
                    var wallet = await _uow.Wallets.GetAsync(w => w.UserId == course.CreatedBy);
                    if (wallet == null)
                    {
                        wallet = new Wallet
                        {
                            WalletId = Guid.NewGuid(),
                            UserId = course.CreatedBy,
                            Balance = 0,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _uow.Wallets.AddAsync(wallet);
                    }

                    wallet.Balance += instructorAmount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    var walletTransaction = new WalletTransaction
                    {
                        WalletTransactionId = Guid.NewGuid(),
                        WalletId = wallet.WalletId,
                        PaymentId = payment.PaymentId,
                        Amount = instructorAmount,
                        TransactionType = 0,
                        CreatedAt = DateTime.UtcNow,
                        BalanceAfterTransaction = wallet.Balance,
                        Description = $"Course sale: {course.Title}"
                    };

                    await _uow.WalletTransactions.AddAsync(walletTransaction);
                }

                _uow.Payments.Update(payment);

                await _uow.CommitAsync();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task ExpirePendingPaymentAsync()
        {
            var expired = await _uow.Payments.GetExpired();

            foreach (var p in expired)
                p.Status = 3;

            await _uow.SaveChangeAsync();
        }

        public async Task<ApiResponse> GetSuccessfulPaymentsAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var paymentList = await _uow.Payments.GetAllAsync(p => p.Status == 0);

                // Map to DTO so business layer doesn't expose data entities to presentation
                var result = paymentList.Select(p => new PaymentRecord { Amount = p.Amount, PaidAt = p.CreatedAt }).ToList();
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }
    }
}
