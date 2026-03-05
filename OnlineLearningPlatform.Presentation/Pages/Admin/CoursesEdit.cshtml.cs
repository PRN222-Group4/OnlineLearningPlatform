//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using OnlineLearningPlatform.BusinessObject.IServices;
//using OnlineLearningPlatform.BusinessObject.Requests.Course;

//namespace OnlineLearningPlatform.Presentation.Pages.Admin
//{
//    public class CoursesEditModel : PageModel
//    {
//        private readonly ICourseService _service;

//        public CoursesEditModel(ICourseService service)
//        {
//            _service = service;
//        }

//        [BindProperty]
//        public UpdateCourseRequest Request { get; set; }

//        public async Task OnGetAsync(Guid id)
//        {
//            var resp = await _service.GetCourseByIdAsync(id);
//            if (resp != null && resp.IsSuccess && resp.Result is OnlineLearningPlatform.BusinessObject.Responses.Course.CourseResponse cr)
//            {
//                Request = new UpdateCourseRequest
//                {
//                    CourseId = cr.CourseId,
//                    Title = cr.Title,
//                    Description = cr.Description,
//                    Price = cr.Price
//                };
//            }
//        }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            if (!ModelState.IsValid) return Page();
//            var resp = await _service.UpdateCourseAsync(Request);
//            if (resp != null && resp.IsSuccess)
//            {
//                return RedirectToPage("/Admin/Courses");
//            }
//            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Update failed");
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
    public class CoursesEditModel : PageModel
    {
        private readonly ICourseService _service;
        public CoursesEditModel(ICourseService service) => _service = service;

        [BindProperty]
        public UpdateCourseRequest Request { get; set; } = new();

        public string? CurrentImage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var resp = await _service.GetCourseByIdAsync(id);
            if (resp?.IsSuccess == true && resp.Result is CourseResponse cr)
            {
                CurrentImage = cr.Image;
                Request = new UpdateCourseRequest
                {
                    CourseId = cr.CourseId,
                    Title = cr.Title,
                    Description = cr.Description,
                    Price = cr.Price
                };
                return Page();
            }
            return RedirectToPage("/Admin/Courses");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var resp = await _service.UpdateCourseAsync(Request);
            if (resp?.IsSuccess == true)
            {
                TempData["Toast"] = "Course updated successfully.";
                TempData["ToastType"] = "success";
                return RedirectToPage("/Admin/Courses");
            }

            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Update failed.");
            return Page();
        }
    }
}