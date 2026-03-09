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
        private readonly IEnrollmentService _enrollmentService;
        private readonly IClaimService _claimService;

        public DetailModel(ICourseService courseService, IPaymentService paymentService, IEnrollmentService enrollmentService, IClaimService claimService)
        {
            _courseService = courseService;
            _paymentService = paymentService;
            _enrollmentService = enrollmentService;
            _claimService = claimService;
        }

        public CourseDetailResponse? Course { get; set; }

        public Guid CourseId { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        public bool IsEnrolled { get; set; }
        public int UserRole { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            CourseId = id;

            var claim = _claimService.GetUserClaim();
            UserRole = claim.Role;

            var resp = await _courseService.GetCourseDetailAsync(id);
            if (resp != null && resp.IsSuccess && resp.Result != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(resp.Result);
                Course = System.Text.Json.JsonSerializer.Deserialize<CourseDetailResponse>(json);
                if (Course != null) Amount = Course.Price;

                var enrollCheck = await _enrollmentService.CheckUserEnrollmentAsync(claim.UserId, id);
                IsEnrolled = enrollCheck?.IsSuccess == true && (bool)enrollCheck.Result!;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostPayAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Error"] = "Lỗi xác thực khóa học. Tham số rỗng!";
                return RedirectToPage("/Courses/Index");
            }

            if (Amount <= 0)
            {
                var enrollResp = await _enrollmentService.EnrollStudentDirectlyAsync(id);
                if (enrollResp?.IsSuccess == true)
                {
                    return RedirectToPage("/Student/Learn", new { courseId = id });
                }

                TempData["Error"] = enrollResp?.ErrorMessage ?? "Enrollment failed.";
                return RedirectToPage(new { id = id });
            }

            var resp = await _paymentService.CreatePayOSPaymentAsync(
                new OnlineLearningPlatform.BusinessObject.Requests.Payment.CreateNewPaymentRequest
                {
                    CourseId = id,
                    Amount = Amount
                });

            if (resp != null) return Redirect(resp.CheckoutUrl);

            TempData["Error"] = resp?.CheckoutUrl ?? "Payment failed";
            return RedirectToPage(new { id = id });
        }
    }
}