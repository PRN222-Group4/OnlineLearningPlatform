using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Wallet
{
    public class IndexModel : PageModel
    {
        private readonly IWalletService _walletService;

        public IndexModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public WalletResponse MyWallet { get; set; } = new WalletResponse();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var res = await _walletService.GetMyWalletAsync();
            if (res.IsSuccess && res.Result != null)
            {
                var json = JsonSerializer.Serialize(res.Result);
                MyWallet = JsonSerializer.Deserialize<WalletResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new WalletResponse();
            }
            else
            {
                ErrorMessage = res.ErrorMessage;
            }
        }

        public async Task<IActionResult> OnPostWithdrawAsync(decimal amount, string bankInfo)
        {

            if (amount < 50000)
            {
                ErrorMessage = "Minimum withdrawal amount is 50,000 ₫.";
                await OnGetAsync();
                return Page();
            }

            var res = await _walletService.RequestWithdrawalAsync(amount, bankInfo);

            if (res.IsSuccess)
            {
                TempData["Success"] = "Payout request submitted! Admin will process it shortly.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = res.ErrorMessage;
                await OnGetAsync();
                return Page();
            }
        }
    }
}