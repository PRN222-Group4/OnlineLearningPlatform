using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PayoutsModel : PageModel
    {
        private readonly IWalletService _walletService;
        private readonly IEmailService _emailService; // Gắn Email Service vào đây

        public PayoutsModel(IWalletService walletService, IEmailService emailService)
        {
            _walletService = walletService;
            _emailService = emailService;
        }

        public decimal TotalGrossSales { get; set; }
        public decimal PlatformRevenue { get; set; }

        public List<PendingPayoutResponse> PendingPayouts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var revRes = await _walletService.GetPlatformRevenueAsync();
            if (revRes.IsSuccess && revRes.Result != null)
            {
                var revJson = JsonSerializer.Serialize(revRes.Result);
                using var doc = JsonDocument.Parse(revJson);
                TotalGrossSales = doc.RootElement.GetProperty("TotalGrossSales").GetDecimal();
                PlatformRevenue = doc.RootElement.GetProperty("PlatformRevenue").GetDecimal();
            }

            var payoutRes = await _walletService.GetPendingPayoutsAsync();
            if (payoutRes.IsSuccess && payoutRes.Result != null)
            {
                PendingPayouts = payoutRes.Result as List<PendingPayoutResponse> ?? new List<PendingPayoutResponse>();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid walletId)
        {
            await OnGetAsync();
            var teacherInfo = PendingPayouts.FirstOrDefault(p => p.WalletId == walletId);

            var res = await _walletService.ApprovePayoutAsync(walletId);
            if (res.IsSuccess)
            {
                if (teacherInfo != null && !string.IsNullOrEmpty(teacherInfo.InstructorEmail))
                {
                    await _emailService.SendPayoutApprovedEmail(
                        teacherInfo.InstructorName,
                        teacherInfo.InstructorEmail,
                        teacherInfo.PendingBalance
                    );
                }

                TempData["Success"] = "Payout approved! The funds have been deducted and email sent.";
            }
            else
            {
                TempData["Error"] = res.ErrorMessage;
            }
            return RedirectToPage();
        }
    }
}