using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class CoursesCreateModel : PageModel
    {
        private readonly ICourseService _service;

        public CoursesCreateModel(ICourseService service)
        {
            _service = service;
        }

        [BindProperty]
        public CreateNewCourseRequest Request { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var resp = await _service.CreateNewCourseAsync(Request);
            if (resp != null && resp.IsSuccess)
            {
                return RedirectToPage("/Admin/Courses");
            }
            ModelState.AddModelError(string.Empty, resp?.ErrorMessage ?? "Create failed");
            return Page();
        }
    }
}