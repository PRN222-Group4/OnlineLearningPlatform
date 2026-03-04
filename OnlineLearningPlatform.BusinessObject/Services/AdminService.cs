using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;
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

        // ?? Overview ??????????????????????????????????????????????????????????
        public async Task<AdminOverviewResponse> GetOverviewAsync(int recentPayments = 10)
        {
            var resp = new AdminOverviewResponse();
            var users = await _uow.Users.GetAllAsync(u => true);
            var courses = await _uow.Courses.GetAllAsync(c => !c.IsDeleted);
            var enrollments = await _uow.Enrollments.GetAllAsync(e => true);
            var payments = await _uow.Payments.GetAllAsync(p => p.Status == 1 && p.PaidAt != null);

            resp.TotalUsers = users.Count;
            resp.TotalCourses = courses.Count;
            resp.TotalEnrollments = enrollments.Count;
            resp.TotalRevenue = payments.Sum(p => p.Amount);

            var topCourse = await _paymentService.GetTopCourseByEnrollmentsAsync();
            if (topCourse != null) { resp.TopCourseTitle = topCourse.Title; resp.TopCourseEnrolls = topCourse.EnrollCount; }

            var topInstructor = await _paymentService.GetTopInstructorByStudentsAsync();
            if (topInstructor != null) { resp.TopInstructorName = topInstructor.InstructorName; resp.TopInstructorStudents = topInstructor.StudentCount; }

            resp.RecentPayments = payments.OrderByDescending(p => p.PaidAt).Take(recentPayments)
                .Select(p => new OnlineLearningPlatform.BusinessObject.Responses.Payment.PaymentRecord
                {
                    Amount = p.Amount,
                    PaidAt = p.PaidAt,
                    UserEmail = p.UserId != null ? _uow.Users.GetAsync(u => u.UserId == p.UserId).Result?.Email : string.Empty
                }).ToList();

            return resp;
        }

        // ?? Get Users (list + search + filter + paging) ???????????????????????
        public async Task<AdminUsersResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
        {
            var all = await _uow.Users.GetAllAsync(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                all = all.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "all")
            {
                var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    { { "Admin", 0 }, { "Instructor", 1 }, { "Student", 2 } };
                if (map.TryGetValue(role, out var r)) all = all.Where(u => u.Role == r).ToList();
            }

            var total = all.Count;
            var paged = all.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => MapToItem(u)).ToList();

            return new AdminUsersResponse { Users = paged, TotalCount = total, Page = page, PageSize = pageSize };
        }

        // ?? Get single user ???????????????????????????????????????????????????
        public async Task<AdminUserItem?> GetUserByIdAsync(Guid userId)
        {
            var u = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            return u == null ? null : MapToItem(u);
        }

        // ?? Update user ???????????????????????????????????????????????????????
        public async Task<bool> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request)
        {
            var u = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (u == null) return false;

            u.FullName = request.FullName;
            u.PhoneNumber = request.PhoneNumber;
            u.Bio = request.Bio;
            u.Title = request.Title;
            u.Role = request.Role;
            u.UpdatedAt = DateTime.UtcNow;
            u.UpdatedBy = userId; // updated by admin; pass admin id if available

            _uow.Users.Update(u);
            await _uow.SaveChangeAsync();
            return true;
        }

        // ?? Soft delete ???????????????????????????????????????????????????????
        public async Task<bool> SoftDeleteUserAsync(Guid userId)
        {
            var u = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (u == null) return false;

            u.IsDeleted = true;
            u.UpdatedAt = DateTime.UtcNow;

            _uow.Users.Update(u);
            await _uow.SaveChangeAsync();
            return true;
        }

        // ?? Toggle ban (IsVerfied toggle) ?????????????????????????????????????
        public async Task<bool> ToggleBanUserAsync(Guid userId)
        {
            var u = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (u == null) return false;

            u.IsVerfied = !u.IsVerfied;
            u.UpdatedAt = DateTime.UtcNow;

            _uow.Users.Update(u);
            await _uow.SaveChangeAsync();
            return true;
        }

        // ?? Helpers ???????????????????????????????????????????????????????????
        private static AdminUserItem MapToItem(OnlineLearningPlatform.DataAccess.Entities.User u) => new()
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Image = u.Image,
            PhoneNumber = u.PhoneNumber,
            Bio = u.Bio,
            Title = u.Title,
            Role = u.Role,
            RoleName = u.Role switch { 0 => "Admin", 1 => "Instructor", 2 => "Student", _ => "User" },
            IsVerified = u.IsVerfied,
            IsDeleted = u.IsDeleted,
            CreatedAt = u.CreatedAt.ToLocalTime()
        };
    }
}