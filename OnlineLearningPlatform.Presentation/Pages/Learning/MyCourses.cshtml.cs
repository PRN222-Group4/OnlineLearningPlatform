using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.Presentation.Pages.Learning
{
    [Authorize]
    public class MyCoursesModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/Student/MyCourses");
        }
    }
}
