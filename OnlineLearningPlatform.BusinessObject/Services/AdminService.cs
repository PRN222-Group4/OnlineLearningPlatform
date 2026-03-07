using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPaymentService _paymentService;

        public AdminService(IUnitOfWork uow, IPaymentService paymentService)
        {
            _uow = uow;
            _paymentService = paymentService;
        }

        public async Task<AdminOverviewResponse> GetOverviewAsync(int recentPayments = 10)
        {
            var response = new AdminOverviewResponse();

            var users = await _uow.Users.GetAllAsync(u => true);
            var courses = await _uow.Courses.GetAllAsync(c => !c.IsDeleted);
            var enrollments = await _uow.Enrollments.GetAllAsync(e => !e.IsDeleted);
            var successfulPayments = await _uow.Payments.GetAllAsync(p => p.Status == 1 && p.PaidAt != null && !p.IsDeleted);
            var recentPaymentRows = await _uow.Payments.GetRecentForAdminAsync(recentPayments);

            response.TotalUsers = users.Count;
            response.TotalCourses = courses.Count;
            response.TotalEnrollments = enrollments.Count;
            response.TotalRevenue = successfulPayments.Sum(p => p.Amount);

            var topCourse = await _paymentService.GetTopCourseByEnrollmentsAsync();
            if (topCourse != null)
            {
                response.TopCourseTitle = topCourse.Title;
                response.TopCourseEnrolls = topCourse.EnrollCount;
            }

            var topInstructor = await _paymentService.GetTopInstructorByStudentsAsync();
            if (topInstructor != null)
            {
                response.TopInstructorName = topInstructor.InstructorName;
                response.TopInstructorStudents = topInstructor.StudentCount;
            }

            response.RecentPayments = recentPaymentRows.Select(p => new PaymentRecord
            {
                PaymentId = p.PaymentId,
                OrderCode = p.OrderCode,
                StudentName = p.User?.FullName,
                UserEmail = p.User?.Email,
                CourseTitle = p.Course?.Title,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ExpiredAt = p.ExpiredAt,
                PaidAt = p.PaidAt
            }).ToList();

            return response;
        }

        public async Task<AdminDashboardResponse> GetDashboardAsync(int year)
        {
            var response = new AdminDashboardResponse();
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var payments = await _uow.Payments.GetAllAsync(p =>
                p.Status == 1 && p.PaidAt != null && p.PaidAt.Value.Year == year && !p.IsDeleted);

            response.RevenueMonths = monthNames.ToList();
            response.RevenueData = Enumerable.Range(1, 12)
                .Select(m => payments.Where(p => p.PaidAt!.Value.Month == m).Sum(p => p.Amount))
                .ToList();

            var enrollments = await _uow.Enrollments.GetAllAsync(e =>
                !e.IsDeleted && e.EnrolledAt != null && e.EnrolledAt.Value.Year == year);

            response.EnrollmentMonths = monthNames.ToList();
            response.EnrollmentData = Enumerable.Range(1, 12)
                .Select(m => enrollments.Count(e => e.EnrolledAt!.Value.Month == m))
                .ToList();

            var users = await _uow.Users.GetAllAsync(u => !u.IsDeleted);
            response.AdminCount = users.Count(u => u.Role == 0);
            response.InstructorCount = users.Count(u => u.Role == 1);
            response.StudentCount = users.Count(u => u.Role == 2);

            var allEnrollments = await _uow.Enrollments.GetAllAsync(e => !e.IsDeleted);
            var courses = await _uow.Courses.GetAllAsync(c => !c.IsDeleted);

            var top5 = allEnrollments
                .GroupBy(e => e.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            foreach (var item in top5)
            {
                var course = courses.FirstOrDefault(c => c.CourseId == item.CourseId);
                if (course == null)
                {
                    continue;
                }

                var title = course.Title.Length > 30 ? course.Title[..30] + "..." : course.Title;
                response.TopCourseTitles.Add(title);
                response.TopCourseEnrolls.Add(item.Count);
            }

            return response;
        }

        public async Task<AdminUsersResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
        {
            var all = await _uow.Users.GetAllAsync(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                all = all
                    .Where(u => u.FullName.ToLower().Contains(normalizedSearch) || u.Email.ToLower().Contains(normalizedSearch))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "all")
            {
                var roleMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Admin", 0 },
                    { "Instructor", 1 },
                    { "Student", 2 }
                };

                if (roleMap.TryGetValue(role, out var roleValue))
                {
                    all = all.Where(u => u.Role == roleValue).ToList();
                }
            }

            var total = all.Count;
            var paged = all
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToItem)
                .ToList();

            return new AdminUsersResponse
            {
                Users = paged,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdminUserItem?> GetUserByIdAsync(Guid userId)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            return user == null ? null : MapToItem(user);
        }

        public async Task<bool> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null)
            {
                return false;
            }

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Bio = request.Bio;
            user.Title = request.Title;
            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow;

            _uow.Users.Update(user);
            await _uow.SaveChangeAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(Guid userId)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null)
            {
                return false;
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            _uow.Users.Update(user);
            await _uow.SaveChangeAsync();
            return true;
        }

        public async Task<bool> ToggleBanUserAsync(Guid userId)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null)
            {
                return false;
            }

            user.IsVerfied = !user.IsVerfied;
            user.UpdatedAt = DateTime.UtcNow;

            _uow.Users.Update(user);
            await _uow.SaveChangeAsync();
            return true;
        }

        private static AdminUserItem MapToItem(OnlineLearningPlatform.DataAccess.Entities.User user) => new()
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Image = user.Image,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            Title = user.Title,
            Role = user.Role,
            RoleName = user.Role switch
            {
                0 => "Admin",
                1 => "Instructor",
                2 => "Student",
                _ => "User"
            },
            IsVerified = user.IsVerfied,
            IsDeleted = user.IsDeleted,
            CreatedAt = user.CreatedAt.ToLocalTime()
        };
    }
}
