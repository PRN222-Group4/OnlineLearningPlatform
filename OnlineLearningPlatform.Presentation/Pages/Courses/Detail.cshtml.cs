using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Courses
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IPaymentService _paymentService;

        public DetailModel(ICourseService courseService, IPaymentService paymentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
        }

        [BindProperty]
        public CourseDetailResponse? Course { get; set; }

        [BindProperty]
        public Guid CourseId { get; set; }

        public async Task OnGetAsync(Guid id)
        {
            var resp = await _courseService.GetCourseDetailAsync(id);
            if (resp != null && resp.IsSuccess && resp.Result != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(resp.Result);
                Course = System.Text.Json.JsonSerializer.Deserialize<CourseDetailResponse>(json);
                CourseId = id;
            }
        }

        public async Task<IActionResult> OnPostPayAsync()
        {
            // Amount is now determined server-side from course.Price — no client input
            var resp = await _paymentService.CreatePayOSPaymentAsync(new OnlineLearningPlatform.BusinessObject.Requests.Payment.CreateNewPaymentRequest
            {
                CourseId = CourseId
            });

            if (resp != null)
            {
                return Redirect(resp.CheckoutUrl);
            }

            TempData["Error"] = resp?.CheckoutUrl ?? "Payment failed";
            return Page();
        }
    }
}