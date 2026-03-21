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

        [BindProperty(SupportsGet = true)]
        public string? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Quarter { get; set; }

        public string DebugInfo { get; set; } = "";

        public async Task OnGetAsync()
        {
            try
            {
                DateTime? from = null, to = null;
                if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParse(FromDate, out var fd)) from = fd;
                if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParse(ToDate, out var td)) to = td;

                Data = await _adminService.GetDashboardAsync(Year, fromDate: from, toDate: to, quarter: Quarter);
                DebugInfo = $"Growth={Data.RevenueGrowth}, Current={Data.RevenueData.Sum()}, Enrolls={Data.EnrollmentGrowth}";
            }
            catch (Exception ex)
            {
                ViewData["DebugError"] = ex.ToString();
                Data = new AdminDashboardResponse();
            }

            var finRes = await _walletService.GetCashflowReportAsync();
            if (finRes.IsSuccess && finRes.Result != null)
            {
                var json = JsonSerializer.Serialize(finRes.Result);
                Financials = JsonSerializer.Deserialize<CashflowReportResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
    }
}