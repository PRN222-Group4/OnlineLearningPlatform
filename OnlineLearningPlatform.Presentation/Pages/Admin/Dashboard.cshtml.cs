using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IAdminService _adminService;
        public DashboardModel(IAdminService adminService) => _adminService = adminService;

        public AdminDashboardResponse Data { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        public async Task OnGetAsync()
        {
            try { Data = await _adminService.GetDashboardAsync(Year); }
            catch { Data = new AdminDashboardResponse(); }
        }
    }
}