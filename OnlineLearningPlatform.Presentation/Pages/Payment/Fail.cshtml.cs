using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.Presentation.Pages.Payment
{
    public class FailModel : PageModel
    {
        public string? OrderCode { get; set; }

        public IActionResult OnGet(string? code, string? id, bool? cancel, string? status, long? orderCode)
        {
            OrderCode = orderCode?.ToString();
            return Page();
        }
    }
}