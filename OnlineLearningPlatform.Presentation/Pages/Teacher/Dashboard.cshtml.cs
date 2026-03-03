using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Collections.Generic;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher
{
    public class DashboardModel : PageModel
    {
        private readonly ICourseService _courseService;

        public DashboardModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public List<CourseResponse>? Courses { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var resp = await _courseService.GetCoursesByInstructorAsync();
                if (resp != null && resp.IsSuccess && resp.Result != null)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(resp.Result);
                    Courses = System.Text.Json.JsonSerializer.Deserialize<List<CourseResponse>>(json);
                }
                else
                {
                    ErrorMessage = resp?.ErrorMessage ?? "Failed to load courses";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
