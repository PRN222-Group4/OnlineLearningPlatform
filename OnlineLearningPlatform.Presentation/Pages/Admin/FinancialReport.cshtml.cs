using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class FinancialReportModel : PageModel
    {
        private readonly IWalletService _walletService;

        public FinancialReportModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public CashflowReportResponse Report { get; set; } = new();

        public async Task OnGetAsync()
        {
            var res = await _walletService.GetCashflowReportAsync();
            if (res.IsSuccess && res.Result != null)
            {
                var json = JsonSerializer.Serialize(res.Result);
                Report = JsonSerializer.Deserialize<CashflowReportResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
    }
}