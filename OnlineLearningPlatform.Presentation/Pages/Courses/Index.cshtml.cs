using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Courses
{
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty(SupportsGet = true)]
        public CourseFilterRequest Filter { get; set; } = new();

        public PaginatedCourseResponse CourseData { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (Filter.PageNumber < 1) Filter.PageNumber = 1;

            var response = await _courseService.GetFilteredCoursesAsync(Filter);

            if (response.IsSuccess && response.Result != null)
            {
                CourseData = (PaginatedCourseResponse)response.Result;
            }

            return Page();
        }

        public string GetLevelString(int level) => level switch
        {
            0 => "Beginner",
            1 => "Intermediate",
            2 => "Advanced",
            3 => "All Levels",
            _ => "Unknown"
        };
    }
}