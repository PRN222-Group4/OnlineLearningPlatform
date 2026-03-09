using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly IWalletService _walletService;

        public DashboardModel(IAdminService adminService, IWalletService walletService)
        {
            _adminService = adminService;
            _walletService = walletService;
        }

        public AdminDashboardResponse Data { get; set; } = new();
        public CashflowReportResponse Financials { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        public async Task OnGetAsync()
        {
            try { Data = await _adminService.GetDashboardAsync(Year); }
            catch { Data = new AdminDashboardResponse(); }
            var finRes = await _walletService.GetCashflowReportAsync();
            if (finRes.IsSuccess && finRes.Result != null)
            {
                var json = JsonSerializer.Serialize(finRes.Result);
                Financials = JsonSerializer.Deserialize<CashflowReportResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
    }
}