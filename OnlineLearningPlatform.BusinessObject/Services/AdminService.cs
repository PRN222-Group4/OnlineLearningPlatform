using Microsoft.EntityFrameworkCore;
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
            var enrollments = await _uow.Enrollments.GetAllAsync(e => true);

            // Lấy tất cả payments (Paid + Failed) cho RecentPayments table
            // Include User + Course để load navigation properties
            var allPayments = await _uow.Payments.GetQueryable()
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Course)
                .ToListAsync();

            // TotalRevenue chỉ tính Paid (Status == 1)
            var paidPayments = allPayments.Where(p => p.Status == 1).ToList();

            response.TotalUsers = users.Count;
            response.TotalCourses = courses.Count;
            response.TotalEnrollments = enrollments.Count;
            response.TotalRevenue = paidPayments.Sum(p => p.Amount);

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

            response.RecentPayments = allPayments
                .Take(recentPayments)
                .Select(p => new PaymentRecord
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


        public async Task<AdminDashboardResponse> GetDashboardAsync(int year, int? month = null, int? day = null, DateTime? fromDate = null, DateTime? toDate = null, int? quarter = null)
        {
            var response = new AdminDashboardResponse();

            // Xác định khoảng thời gian
            DateTime start, end;
            if (fromDate.HasValue && toDate.HasValue)
            {
                start = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                end = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            }
            else if (quarter.HasValue)
            {
                var qStart = new DateTime(year, (quarter.Value - 1) * 3 + 1, 1);
                var qEnd = qStart.AddMonths(3).AddTicks(-1);
                start = DateTime.SpecifyKind(qStart, DateTimeKind.Utc);
                end = DateTime.SpecifyKind(qEnd, DateTimeKind.Utc);
            }
            else
            {
                start = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);
                end = DateTime.SpecifyKind(new DateTime(year, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            }

            // Khoảng thời gian năm trước (cùng kỳ)
            DateTime prevStart, prevEnd;
            if (fromDate.HasValue && toDate.HasValue)
            {
                var duration = end - start;
                prevStart = DateTime.SpecifyKind(start.AddYears(-1), DateTimeKind.Utc);
                prevEnd = DateTime.SpecifyKind(end.AddYears(-1), DateTimeKind.Utc);
            }
            else if (quarter.HasValue)
            {
                var qStart = new DateTime(year - 1, (quarter.Value - 1) * 3 + 1, 1);
                prevStart = DateTime.SpecifyKind(qStart, DateTimeKind.Utc);
                prevEnd = DateTime.SpecifyKind(qStart.AddMonths(3).AddTicks(-1), DateTimeKind.Utc);
            }
            else
            {
                prevStart = DateTime.SpecifyKind(new DateTime(year - 1, 1, 1), DateTimeKind.Utc);
                prevEnd = DateTime.SpecifyKind(new DateTime(year - 1, 12, 31, 23, 59, 59), DateTimeKind.Utc);
            }

            response.PrevPeriodLabel = quarter.HasValue
                ? $"Q{quarter} {year - 1}"
                : fromDate.HasValue
                    ? $"{prevStart:dd/MM/yy}–{prevEnd:dd/MM/yy}"
                    : $"{year - 1}";

            var allPayments = await _uow.Payments.GetAllAsync(p =>
                p.Status == 1 && p.PaidAt != null &&
                p.PaidAt.Value >= start && p.PaidAt.Value <= end);

            var allEnrollments = await _uow.Enrollments.GetAllAsync(e =>
                !e.IsDeleted && e.EnrolledAt != null &&
                e.EnrolledAt.Value >= start && e.EnrolledAt.Value <= end);

            var prevPayments = await _uow.Payments.GetAllAsync(p =>
                p.Status == 1 && p.PaidAt != null &&
                p.PaidAt.Value >= prevStart && p.PaidAt.Value <= prevEnd);

            var prevEnrollments = await _uow.Enrollments.GetAllAsync(e =>
                !e.IsDeleted && e.EnrolledAt != null &&
                e.EnrolledAt.Value >= prevStart && e.EnrolledAt.Value <= prevEnd);

            var currentRevenue = allPayments.Sum(p => p.Amount);
            var prevRevenue = prevPayments.Sum(p => p.Amount);
            var currentEnrolls = allEnrollments.Count();
            var prevEnrolls = prevEnrollments.Count();

            response.RevenueGrowth = prevRevenue == 0 ? null : Math.Round((currentRevenue - prevRevenue) / prevRevenue * 100, 1);
            response.EnrollmentGrowth = prevEnrolls == 0 ? null : Math.Round((decimal)(currentEnrolls - prevEnrolls) / prevEnrolls * 100, 1);

            var totalDays = (end - start).TotalDays;
            var isFullYear = !fromDate.HasValue && !toDate.HasValue && !quarter.HasValue;
            var isFullQuarter = quarter.HasValue && !fromDate.HasValue;

            // ── Bucketing logic ──
            List<string> labels;
            List<decimal> revCurrent, revPrev;
            List<int> enrollCurrent, enrollPrev;

            if (totalDays <= 1)
            {
                var slots = Enumerable.Range(0, 12).ToList();
                labels = slots.Select(i => $"{i * 2:00}h-{i * 2 + 2:00}h").ToList();
                revCurrent = slots.Select(i => allPayments.Where(p => p.PaidAt!.Value.Hour >= i * 2 && p.PaidAt.Value.Hour < i * 2 + 2).Sum(p => p.Amount)).ToList();
                revPrev = slots.Select(i => prevPayments.Where(p => p.PaidAt!.Value.Hour >= i * 2 && p.PaidAt.Value.Hour < i * 2 + 2).Sum(p => p.Amount)).ToList();
                enrollCurrent = slots.Select(i => allEnrollments.Count(e => e.EnrolledAt!.Value.Hour >= i * 2 && e.EnrolledAt.Value.Hour < i * 2 + 2)).ToList();
                enrollPrev = slots.Select(i => prevEnrollments.Count(e => e.EnrolledAt!.Value.Hour >= i * 2 && e.EnrolledAt.Value.Hour < i * 2 + 2)).ToList();
            }
            else if (isFullYear)
            {
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                labels = monthNames.ToList();
                revCurrent = Enumerable.Range(1, 12).Select(m => allPayments.Where(p => p.PaidAt!.Value.Month == m).Sum(p => p.Amount)).ToList();
                revPrev = Enumerable.Range(1, 12).Select(m => prevPayments.Where(p => p.PaidAt!.Value.Month == m).Sum(p => p.Amount)).ToList();
                enrollCurrent = Enumerable.Range(1, 12).Select(m => allEnrollments.Count(e => e.EnrolledAt!.Value.Month == m)).ToList();
                enrollPrev = Enumerable.Range(1, 12).Select(m => prevEnrollments.Count(e => e.EnrolledAt!.Value.Month == m)).ToList();
            }
            else if (isFullQuarter)
            {
                // Quarter → 3 tháng
                var qStartMonth = (quarter!.Value - 1) * 3 + 1;
                var months = Enumerable.Range(qStartMonth, 3).ToList();
                var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                labels = months.Select(m => monthNames[m]).ToList();
                revCurrent = months.Select(m => allPayments.Where(p => p.PaidAt!.Value.Month == m).Sum(p => p.Amount)).ToList();
                revPrev = months.Select(m => prevPayments.Where(p => p.PaidAt!.Value.Month == m).Sum(p => p.Amount)).ToList();
                enrollCurrent = months.Select(m => allEnrollments.Count(e => e.EnrolledAt!.Value.Month == m)).ToList();
                enrollPrev = months.Select(m => prevEnrollments.Count(e => e.EnrolledAt!.Value.Month == m)).ToList();
            }
            else if (totalDays <= 12)
            {
                var days = Enumerable.Range(0, (int)Math.Ceiling(totalDays)).Select(i => start.AddDays(i).Date).ToList();
                labels = days.Select(d => d.ToString("dd/MM")).ToList();
                revCurrent = days.Select(d => allPayments.Where(p => p.PaidAt!.Value.Date == d).Sum(p => p.Amount)).ToList();
                revPrev = days.Select(d => prevPayments.Where(p => p.PaidAt!.Value.Date == d.AddYears(-1)).Sum(p => p.Amount)).ToList();
                enrollCurrent = days.Select(d => allEnrollments.Count(e => e.EnrolledAt!.Value.Date == d)).ToList();
                enrollPrev = days.Select(d => prevEnrollments.Count(e => e.EnrolledAt!.Value.Date == d.AddYears(-1))).ToList();
            }
            else
            {
                var actualMonths = new List<(int year, int month)>();
                var cur = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                while (cur <= end) { actualMonths.Add((cur.Year, cur.Month)); cur = cur.AddMonths(1); }

                if (actualMonths.Count <= 12)
                {
                    var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    labels = actualMonths.Select(m => monthNames[m.month]).ToList();
                    revCurrent = actualMonths.Select(m => allPayments.Where(p => p.PaidAt!.Value.Month == m.month && p.PaidAt.Value.Year == m.year).Sum(p => p.Amount)).ToList();
                    revPrev = actualMonths.Select(m => prevPayments.Where(p => p.PaidAt!.Value.Month == m.month && p.PaidAt.Value.Year == m.year - 1).Sum(p => p.Amount)).ToList();
                    enrollCurrent = actualMonths.Select(m => allEnrollments.Count(e => e.EnrolledAt!.Value.Month == m.month && e.EnrolledAt.Value.Year == m.year)).ToList();
                    enrollPrev = actualMonths.Select(m => prevEnrollments.Count(e => e.EnrolledAt!.Value.Month == m.month && e.EnrolledAt.Value.Year == m.year - 1)).ToList();
                }
                else
                {
                    var slots = Enumerable.Range(0, 12).Select(i => (from: start.AddDays(i * totalDays / 12), to: start.AddDays((i + 1) * totalDays / 12))).ToList();
                    labels = slots.Select(w => w.from.ToString("dd/MM/yy")).ToList();
                    revCurrent = slots.Select(w => allPayments.Where(p => p.PaidAt!.Value >= w.from && p.PaidAt.Value < w.to).Sum(p => p.Amount)).ToList();
                    revPrev = slots.Select(w => prevPayments.Where(p => p.PaidAt!.Value >= w.from.AddYears(-1) && p.PaidAt.Value < w.to.AddYears(-1)).Sum(p => p.Amount)).ToList();
                    enrollCurrent = slots.Select(w => allEnrollments.Count(e => e.EnrolledAt!.Value >= w.from && e.EnrolledAt.Value < w.to)).ToList();
                    enrollPrev = slots.Select(w => prevEnrollments.Count(e => e.EnrolledAt!.Value >= w.from.AddYears(-1) && e.EnrolledAt.Value < w.to.AddYears(-1))).ToList();
                }
            }

            response.RevenueMonths = labels;
            response.RevenueData = revCurrent;
            response.PrevRevenueData = revPrev;
            response.EnrollmentMonths = labels;
            response.EnrollmentData = enrollCurrent;
            response.PrevEnrollmentData = enrollPrev;

            
            // ── Users ──
            var users = await _uow.Users.GetAllAsync(u => !u.IsDeleted);
            response.AdminCount = users.Count(u => u.Role == 0);
            response.InstructorCount = users.Count(u => u.Role == 1);
            response.StudentCount = users.Count(u => u.Role == 2);

            var allCourses = await _uow.Courses.GetAllAsync(c => !c.IsDeleted);

            Console.WriteLine($"=== DEBUG: start={start}, end={end}");
            Console.WriteLine($"=== DEBUG: allPayments={allPayments.Count}, allEnrollments={allEnrollments.Count}");
            Console.WriteLine($"=== DEBUG: allCourses={allCourses.Count}");

            // ── Top 5 courses by enrollment (filtered by period) ──
            var top5Enroll = allEnrollments
                .GroupBy(e => e.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            foreach (var item in top5Enroll)
            {
                var course = allCourses.FirstOrDefault(c => c.CourseId == item.CourseId);
                if (course == null) continue;
                var title = course.Title.Length > 25 ? course.Title[..25] + "..." : course.Title;
                response.TopCourseTitles.Add(title);
                response.TopCourseEnrolls.Add(item.Count);
            }

            // ── Top 5 courses by revenue (filtered by period) ──
            var top5Revenue = allPayments
                .Where(p => p.CourseId.HasValue)
                .GroupBy(p => p.CourseId!.Value)
                .Select(g => new { CourseId = g.Key, Revenue = g.Sum(p => p.Amount) })
                .OrderByDescending(x => x.Revenue).Take(5).ToList();

            foreach (var item in top5Revenue)
            {
                var course = allCourses.FirstOrDefault(c => c.CourseId == item.CourseId);
                if (course == null) continue;
                var title = course.Title.Length > 25 ? course.Title[..25] + "..." : course.Title;
                response.TopCourseRevenueTitle.Add(title);
                response.TopCourseRevenueData.Add(item.Revenue);
            }

            // ── Top 5 instructors by revenue (filtered by period) ──
            var top5Instructors = allPayments
                .Where(p => p.CourseId.HasValue)
                .GroupBy(p => {
                    var course = allCourses.FirstOrDefault(c => c.CourseId == p.CourseId!.Value);
                    return course?.CreatedBy ?? Guid.Empty;
                })
                .Where(g => g.Key != Guid.Empty)
                .Select(g => new { InstructorId = g.Key, Revenue = g.Sum(p => p.Amount) })
                .OrderByDescending(x => x.Revenue).Take(5).ToList();

            foreach (var item in top5Instructors)
            {
                var instructor = users.FirstOrDefault(u => u.UserId == item.InstructorId);
                if (instructor == null) continue;
                response.TopInstructorNames.Add(instructor.FullName);
                response.TopInstructorRevenue.Add(item.Revenue);
            }

            // ── Top 5 instructors by enrollment (filtered by period) ──
            var top5InstructorEnroll = allEnrollments
                .GroupBy(e => {
                    var course = allCourses.FirstOrDefault(c => c.CourseId == e.CourseId);
                    return course?.CreatedBy ?? Guid.Empty;
                })
                .Where(g => g.Key != Guid.Empty)
                .Select(g => new { InstructorId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            foreach (var item in top5InstructorEnroll)
            {
                var instructor = users.FirstOrDefault(u => u.UserId == item.InstructorId);
                if (instructor == null) continue;
                response.TopInstructorEnrollNames.Add(instructor.FullName);
                response.TopInstructorEnrollData.Add(item.Count);
            }

            // ── User growth theo thời gian ──
            var allUsersRaw = await _uow.Users.GetAllAsync(u => !u.IsDeleted);

            // Convert start/end sang local để so sánh đúng timezone
            var startLocal = start.ToLocalTime();
            var endLocal = end.ToLocalTime();
            var prevStartLocal = prevStart.ToLocalTime();
            var prevEndLocal = prevEnd.ToLocalTime();

            var allUsersInPeriod = allUsersRaw
                .Where(u => u.CreatedAt >= startLocal && u.CreatedAt <= endLocal).ToList();
            var prevUsersInPeriod = allUsersRaw
                .Where(u => u.CreatedAt >= prevStartLocal && u.CreatedAt <= prevEndLocal).ToList();
            Console.WriteLine($"=== DEBUG: allUsersInPeriod={allUsersInPeriod.Count}, prevUsersInPeriod={prevUsersInPeriod.Count}");
            response.UserGrowthLabels = labels;

            if (totalDays <= 1)
            {
                response.UserGrowthData = Enumerable.Range(0, 12)
                    .Select(i => allUsersInPeriod.Count(u => u.CreatedAt.Hour >= i * 2 && u.CreatedAt.Hour < i * 2 + 2)).ToList();
                response.PrevUserGrowthData = Enumerable.Range(0, 12)
                    .Select(i => prevUsersInPeriod.Count(u => u.CreatedAt.Hour >= i * 2 && u.CreatedAt.Hour < i * 2 + 2)).ToList();
            }
            else if (isFullYear)
            {
                response.UserGrowthData = Enumerable.Range(1, 12)
                    .Select(m => allUsersInPeriod.Count(u => u.CreatedAt.Month == m)).ToList();
                response.PrevUserGrowthData = Enumerable.Range(1, 12)
                    .Select(m => prevUsersInPeriod.Count(u => u.CreatedAt.Month == m)).ToList();
            }
            else if (isFullQuarter)
            {
                var qStartMonth = (quarter!.Value - 1) * 3 + 1;
                var months3 = Enumerable.Range(qStartMonth, 3).ToList();
                response.UserGrowthData = months3.Select(m => allUsersInPeriod.Count(u => u.CreatedAt.Month == m)).ToList();
                response.PrevUserGrowthData = months3.Select(m => prevUsersInPeriod.Count(u => u.CreatedAt.Month == m)).ToList();
            }
            else if (totalDays <= 12)
            {
                var days2 = Enumerable.Range(0, (int)Math.Ceiling(totalDays)).Select(i => start.AddDays(i).Date).ToList();
                response.UserGrowthData = days2.Select(d => allUsersInPeriod.Count(u => u.CreatedAt.Date == d)).ToList();
                response.PrevUserGrowthData = days2.Select(d => prevUsersInPeriod.Count(u => u.CreatedAt.Date == d.AddYears(-1))).ToList();
            }
            else
            {
                var actualMonths2 = new List<(int year, int month)>();
                var cur2 = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                while (cur2 <= end) { actualMonths2.Add((cur2.Year, cur2.Month)); cur2 = cur2.AddMonths(1); }

                if (actualMonths2.Count <= 12)
                {
                    response.UserGrowthData = actualMonths2.Select(m => allUsersInPeriod.Count(u => u.CreatedAt.Month == m.month && u.CreatedAt.Year == m.year)).ToList();
                    response.PrevUserGrowthData = actualMonths2.Select(m => prevUsersInPeriod.Count(u => u.CreatedAt.Month == m.month && u.CreatedAt.Year == m.year - 1)).ToList();
                }
                else
                {
                    var slots2 = Enumerable.Range(0, 12).Select(i => (from: start.AddDays(i * totalDays / 12), to: start.AddDays((i + 1) * totalDays / 12))).ToList();
                    response.UserGrowthData = slots2.Select(w => allUsersInPeriod.Count(u => u.CreatedAt >= w.from && u.CreatedAt < w.to)).ToList();
                    response.PrevUserGrowthData = slots2.Select(w => prevUsersInPeriod.Count(u => u.CreatedAt >= w.from.AddYears(-1) && u.CreatedAt < w.to.AddYears(-1))).ToList();
                }
            }
            Console.WriteLine($"=== DEBUG: UserGrowthData={string.Join(",", response.UserGrowthData)}");
            var top5Students = allPayments
    .GroupBy(p => p.UserId)
    .Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Amount) })
    .OrderByDescending(x => x.Total).Take(5).ToList();

            foreach (var item in top5Students)
            {
                var student = users.FirstOrDefault(u => u.UserId == item.UserId);
                if (student == null) continue;
                response.TopStudentNames.Add(student.FullName);
                response.TopStudentSpending.Add(item.Total);
            }


            return response;
        }




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
                var roleMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    { { "Admin", 0 }, { "Instructor", 1 }, { "Student", 2 } };
                if (roleMap.TryGetValue(role, out var rv))
                    all = all.Where(u => u.Role == rv).ToList();
            }

            var total = all.Count;
            var paged = all
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToItem)
                .ToList();

            return new AdminUsersResponse { Users = paged, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<AdminUserItem?> GetUserByIdAsync(Guid userId)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            return user == null ? null : MapToItem(user);
        }

        public async Task<bool> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null) return false;
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
            if (user == null) return false;
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);
            await _uow.SaveChangeAsync();
            return true;
        }

        public async Task<bool> ToggleBanUserAsync(Guid userId)
        {
            var user = await _uow.Users.GetAsync(u => u.UserId == userId && !u.IsDeleted);
            if (user == null) return false;
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
            RoleName = user.Role switch { 0 => "Admin", 1 => "Instructor", 2 => "Student", _ => "User" },
            IsVerified = user.IsVerfied,
            IsDeleted = user.IsDeleted,
            CreatedAt = user.CreatedAt.ToLocalTime()
        };
    }
}