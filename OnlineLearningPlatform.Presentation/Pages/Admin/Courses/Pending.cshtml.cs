using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Admin.Courses
{
    public class PendingModel : PageModel
    {
        private readonly ICourseService _courseService;

        public PendingModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public List<PendingCourseReviewResponse> PendingCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var result = await _courseService.GetPendingCoursesForAdminAsync();
            if (result.IsSuccess && result.Result != null)
            {
                PendingCourses = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new List<PendingCourseReviewResponse>();
            }
        }
    }
}
