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
    }
}