using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher
{
    public class CreateCourseModel : PageModel
    {
        private readonly ICourseService _courseService;

        public CreateCourseModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty]
        public CreateNewCourseRequest Request { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var resp = await _courseService.CreateNewCourseAsync(Request);
            if (resp != null && resp.IsSuccess)
            {
                // redirect to teacher dashboard after create
                return RedirectToPage("/Teacher/Dashboard");
            }

            ErrorMessage = resp?.ErrorMessage ?? "Create failed";
            return Page();
        }
    }
}
