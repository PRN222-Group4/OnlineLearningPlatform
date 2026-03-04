////Courses.cshtml.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using OnlineLearningPlatform.BusinessObject.IServices;
//using OnlineLearningPlatform.BusinessObject.Requests.Course;
//using OnlineLearningPlatform.BusinessObject.Responses.Course;

//namespace OnlineLearningPlatform.Presentation.Pages.Admin
//{
//    public class CoursesModel : PageModel
//    {
//        private readonly ICourseService _service;

//        public CoursesModel(ICourseService service)
//        {
//            _service = service;
//        }

//        public async Task<IActionResult> OnPostSubmitAsync(Guid courseId)
//        {
//            if (courseId == Guid.Empty)
//            {
//                ModelState.AddModelError(string.Empty, "Invalid course");
//                await OnGetAsync();
//                return Page();
//            }

//            var resp = await _service.SubmitCourseForReviewAsync(courseId);
//            if (resp != null && resp.IsSuccess)
//            {
//                return RedirectToPage(new { status = Status });
//            }

//            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Submit failed");
//            await OnGetAsync();
//            return Page();
//        }

//        // allow status to be provided via query string so admin can view different statuses
//        [BindProperty(SupportsGet = true)]
//        public int Status { get; set; }

//        public List<CourseResponse> Courses { get; set; } = new();

//        [BindProperty]
//        public ApproveCourseRequest ApproveRequest { get; set; }

//        public async Task OnGetAsync()
//        {
//            var resp = await _service.GetAllCourseForAdminAsync(Status);
//            if (resp != null && resp.IsSuccess && resp.Result is List<GetAllCourseForAdminResponse> list)
//            {
//                // map to CourseResponse simple view
//                Courses = list.Select(x => new CourseResponse
//                {
//                    CourseId = x.CourseId,
//                    Title = x.Title,
//                    Description = x.Description,
//                    Price = x.Price,
//                    Image = x.Image,
//                    Status = x.Status
//                }).ToList();
//            }
//        }
//        public async Task<IActionResult> OnPostApproveAsync([FromForm] Guid CourseId, [FromForm] bool Status, [FromForm] string? RejectReason)
//        {
//            if (CourseId == Guid.Empty)
//            {
//                ModelState.AddModelError(string.Empty, "Invalid request");
//                await OnGetAsync();
//                return Page();
//            }

//            var req = new ApproveCourseRequest
//            {
//                CourseId = CourseId,
//                Status = Status,
//                RejectReason = RejectReason
//            };

//            var resp = await _service.ApproveCourseAsync(req);
//            if (resp != null && resp.IsSuccess)
//            {
//                return RedirectToPage(new { status = Status });
//            }

//            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Operation failed");
//            await OnGetAsync();
//            return Page();
//        }


//        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
//        {
//            var resp = await _service.DeleteCourseAsync(id);
//            if (resp != null && resp.IsSuccess)
//            {
//                // preserve current Status filter when redirecting back
//                return RedirectToPage(new { status = Status });
//            }
//            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Delete failed");
//            await OnGetAsync();
//            return Page();
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class CoursesModel : PageModel
    {
        private readonly ICourseService _service;
        public CoursesModel(ICourseService service) => _service = service;

        [BindProperty(SupportsGet = true)]
        public int Status { get; set; } = -1;

        public List<CourseResponse> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var resp = await _service.GetAllCourseForAdminAsync(Status);
            if (resp?.IsSuccess == true && resp.Result is List<GetAllCourseForAdminResponse> list)
            {
                Courses = list.Select(x => new CourseResponse
                {
                    CourseId = x.CourseId,
                    Title = x.Title,
                    Description = x.Description,
                    Price = x.Price,
                    Image = x.Image,
                    Status = x.Status
                }).ToList();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid CourseId, bool IsApprove, string? RejectReason)
        {
            if (CourseId == Guid.Empty)
            {
                TempData["Toast"] = "Invalid course.";
                TempData["ToastType"] = "error";
                return RedirectToPage(new { status = Status });
            }

            var req = new ApproveCourseRequest { CourseId = CourseId, Status = IsApprove, RejectReason = RejectReason };
            var resp = await _service.ApproveCourseAsync(req);

            TempData["Toast"] = resp?.IsSuccess == true
                ? (IsApprove ? "Course approved successfully." : "Course rejected.")
                : (resp?.ErrorMessage ?? "Operation failed.");
            TempData["ToastType"] = resp?.IsSuccess == true ? "success" : "error";

            return RedirectToPage(new { status = Status });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var resp = await _service.DeleteCourseAsync(id);
            TempData["Toast"] = resp?.IsSuccess == true ? "Course deleted." : (resp?.ErrorMessage ?? "Delete failed.");
            TempData["ToastType"] = resp?.IsSuccess == true ? "success" : "error";
            return RedirectToPage(new { status = Status });
        }
    }
}
