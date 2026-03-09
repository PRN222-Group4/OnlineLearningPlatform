using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using AutoMapper;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.BusinessObject.Requests.Enrollment;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        //private readonly IPaymentService _paymentService;
        private readonly IClaimService _service;

        public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService service)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _service = service;
        }
        /*
                public async Task<ApiResponse> CreateNewEnrollmentAsync(CreateNewEnrollementRequest request)
                {
                    ApiResponse response = new ApiResponse();
                    try
                    {
                        var claim = _service.GetUserClaim();
                        var existingEnrollment = await _unitOfWork.Enrollments.GetAsync(e => e.CourseId == request.CourseId && e.UserId == claim.UserId);
                        if (existingEnrollment != null)
                        {
                            return response.SetBadRequest(message: "Enrollment had created with Course");
                        }

                        var payment = await _unitOfWork.Payments.GetAsync(p => p.CourseId == request.CourseId && p.UserId == claim.UserId && p.IsSuccess == true);
                        if (payment == null)
                        {
                            return response.SetBadRequest(message: "Payment not found for this course or not success");
                        }

                        var enrollment = _mapper.Map<Enrollment>(request);
                        await _unitOfWork.Enrollments.AddAsync(enrollment);
                        await _unitOfWork.SaveChangeAsync();
                        return response.SetOk("Create");
                    }
                    catch (Exception ex)
                    {
                        return response.SetBadRequest(message: ex.Message);
                    }
                }*/

        public async Task<ApiResponse> EnrollStudentDirectlyAsync(Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var userId = _service.GetUserClaim().UserId;

                var existing = await _unitOfWork.Enrollments.GetAsync(e => e.CourseId == courseId && e.UserId == userId);
                if (existing != null)
                {
                    return response.SetOk(existing.EnrollmentId);
                }

                var newEnrollment = new Enrollment
                {
                    EnrollmentId = Guid.NewGuid(),
                    CourseId = courseId,
                    UserId = userId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = 1,
                    ProgressPercent = 0
                };

                await _unitOfWork.Enrollments.AddAsync(newEnrollment);

                await InitializeLessonProgressAsync(userId, courseId);

                await _unitOfWork.SaveChangeAsync();

                return response.SetOk(newEnrollment.EnrollmentId);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> CheckUserEnrollmentAsync(Guid userId, Guid courseId)
        {
            var response = new ApiResponse();
            try
            {
                var enrollment = await _unitOfWork.Enrollments
                    .GetAsync(e => e.UserId == userId && e.CourseId == courseId && (e.Status == 1 || e.Status == 2));

                return response.SetOk(enrollment != null);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }

        public async Task<ApiResponse> GetStudentEnrollmentsAsync()
        {
            var response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                if (claim.Role != 2)
                {
                    return response.SetBadRequest(message: "Only students can access learning enrollments.");
                }

                var enrollments = await _unitOfWork.Enrollments.GetQueryable()
                    .Include(e => e.Course)
                    .Where(e => e.UserId == claim.UserId && (e.Status == 1 || e.Status == 2) && !e.IsDeleted)
                    .OrderByDescending(e => e.EnrolledAt)
                    .ToListAsync();

                var result = enrollments.Select(e => new StudentEnrollmentSummaryResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course?.Title ?? "Untitled Course",
                    CourseImage = e.Course?.Image,
                    ProgressPercent = e.ProgressPercent,
                    EnrolledAt = e.EnrolledAt
                }).ToList();

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        private async Task InitializeLessonProgressAsync(Guid userId, Guid courseId)
        {
            // Lấy tất cả Module -> Lấy tất cả Lesson -> Tạo record Progress = 0%
            var modules = await _unitOfWork.Modules.GetAllAsync(m => m.CourseId == courseId);

            foreach (var module in modules)
            {
                var lessons = await _unitOfWork.Lessons.GetAllAsync(l => l.ModuleId == module.ModuleId && !l.IsDeleted);

                foreach (var lesson in lessons)
                {
                    var progress = new UserLessonProgress
                    {
                        LessonProgressId = Guid.NewGuid(),
                        UserId = userId,
                        LessonId = lesson.LessonId,
                        IsCompleted = false,
                        CompletionPercent = 0,
                        LastWatchedSecond = 0
                    };
                    await _unitOfWork.UserLessonProgresses.AddAsync(progress);
                }
            }
        }

        public async Task<bool> CheckEnrollmentAsync(Guid courseId)
        {
            try
            {
                var userId = _service.GetUserClaim().UserId;
                var enrollment = await _unitOfWork.Enrollments.GetAsync(
                    e => e.CourseId == courseId && e.UserId == userId && e.Status == 1
                );
                return enrollment != null;
            }
            catch
            {
                return false;
            }
        }

    }
}
