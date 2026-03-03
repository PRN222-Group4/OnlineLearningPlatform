using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using System.Linq;

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
            if (topCourse != null)
            {
                resp.TopCourseTitle = topCourse.Title;
                resp.TopCourseEnrolls = topCourse.EnrollCount;
            }

            var topInstructor = await _paymentService.GetTopInstructorByStudentsAsync();
            if (topInstructor != null)
            {
                resp.TopInstructorName = topInstructor.InstructorName;
                resp.TopInstructorStudents = topInstructor.StudentCount;
            }

            resp.RecentPayments = payments.OrderByDescending(p => p.PaidAt).Take(recentPayments)
                .Select(p => new OnlineLearningPlatform.BusinessObject.Responses.Payment.PaymentRecord
                {
                    Amount = p.Amount,
                    PaidAt = p.PaidAt,
                    UserEmail = p.UserId != null ? _uow.Users.GetAsync(u => u.UserId == p.UserId).Result?.Email : string.Empty
                }).ToList();

            return resp;
        }
    }
}