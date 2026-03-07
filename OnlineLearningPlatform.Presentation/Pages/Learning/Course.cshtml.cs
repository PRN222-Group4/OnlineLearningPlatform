using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.Presentation.Pages.Learning
{
    [Authorize]
    public class CourseModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid CourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? LessonId { get; set; }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Student/Learning", new { courseId = CourseId, lessonId = LessonId });
        }

        public IActionResult OnPostMarkComplete(Guid lessonId)
        {
            return RedirectToPage("/Student/Learning", new { courseId = CourseId, lessonId });
        }
    }
}
