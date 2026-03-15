using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using System.Collections.Generic;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class OverviewModel : PageModel
    {
        private readonly OnlineLearningPlatform.BusinessObject.IServices.IAdminService _adminService;

        public OverviewModel(OnlineLearningPlatform.BusinessObject.IServices.IAdminService adminService)
        {
            _adminService = adminService;
        }

        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }

        public string? TopCourseTitle { get; set; }
        public int TopCourseEnrolls { get; set; }
        public string? TopInstructorName { get; set; }
        public int TopInstructorStudents { get; set; }

        public List<PaymentRecord>? RecentPayments { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var overview = await _adminService.GetOverviewAsync();
                TotalUsers = overview.TotalUsers;
                TotalCourses = overview.TotalCourses;
                TotalEnrollments = overview.TotalEnrollments;
                TotalRevenue = overview.TotalRevenue;
                TopCourseTitle = overview.TopCourseTitle;
                TopCourseEnrolls = overview.TopCourseEnrolls;
                TopInstructorName = overview.TopInstructorName;
                TopInstructorStudents = overview.TopInstructorStudents;
                RecentPayments = overview.RecentPayments;
            }
            catch
            {
                // ignore for now
            }
        }
        public async Task<JsonResult> OnGetStatsJsonAsync()
        {
            try
            {
                var overview = await _adminService.GetOverviewAsync();
                return new JsonResult(new
                {
                    totalUsers = overview.TotalUsers,
                    totalCourses = overview.TotalCourses,
                    totalEnrollments = overview.TotalEnrollments,
                    totalRevenue = overview.TotalRevenue
                });
            }
            catch
            {
                return new JsonResult(new { totalUsers = 0, totalCourses = 0, totalEnrollments = 0, totalRevenue = 0 });
            }
        }
    }
}
