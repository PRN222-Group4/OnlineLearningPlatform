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
        private readonly IEnrollmentService _enrollmentService; // thêm mới

        public DetailModel(ICourseService courseService, IPaymentService paymentService, IEnrollmentService enrollmentService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
            _enrollmentService = enrollmentService;
        }

        [BindProperty]
        public CourseDetailResponse? Course { get; set; }

        [BindProperty]
        public Guid CourseId { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        
        public async Task OnGetAsync(Guid id)
        {
            var resp = await _courseService.GetCourseDetailAsync(id);
            if (resp != null && resp.IsSuccess && resp.Result != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(resp.Result);
                Course = System.Text.Json.JsonSerializer.Deserialize<CourseDetailResponse>(json);
                CourseId = id;
                Amount = Course.Price;
            }
        }

        public async Task<IActionResult> OnPostPayAsync()
        {
            // ── FIX: FREE COURSE (Amount = 0)
            if (Amount <= 0)
            {
                var enrollResp = await _enrollmentService.EnrollStudentDirectlyAsync(CourseId);
                if (enrollResp?.IsSuccess == true)
                    return RedirectToPage("/Learning/MyCourses");

                TempData["Error"] = enrollResp?.ErrorMessage ?? "Enrollment failed.";
                return Page();
            }

            
            var resp = await _paymentService.CreatePayOSPaymentAsync(
                new OnlineLearningPlatform.BusinessObject.Requests.Payment.CreateNewPaymentRequest
                {
                    CourseId = CourseId,
                    Amount = Amount
                });

            if (resp != null)
                return Redirect(resp.CheckoutUrl);

            TempData["Error"] = resp?.CheckoutUrl ?? "Payment failed";
            return Page();
        }
    }
}