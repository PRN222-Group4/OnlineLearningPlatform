using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;

namespace OnlineLearningPlatform.Presentation.Pages.Payment
{
    public class SuccessModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IPaymentService _paymentService;

        public SuccessModel(IEnrollmentService enrollmentService, IPaymentService paymentService)
        {
            _enrollmentService = enrollmentService;
            _paymentService = paymentService;
        }

        public string? CourseId { get; set; }
        public string? OrderCode { get; set; }

        public async Task<IActionResult> OnGetAsync(
            string? code, string? id, bool? cancel, string? status, long? orderCode)
        {
            OrderCode = orderCode?.ToString();
            if (code == "00" && orderCode.HasValue)
            {
                await _paymentService.SyncPaymentStatusAsync(orderCode.Value);
            }

            return Page();
        }
    }
}