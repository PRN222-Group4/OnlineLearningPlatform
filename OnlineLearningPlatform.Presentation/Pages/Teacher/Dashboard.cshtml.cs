using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Text.Json;

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

        public decimal TotalRevenue { get; set; }
        public int TotalStudents { get; set; }
        public int TotalEnrollments { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var resp = await _courseService.GetCoursesByInstructorAsync();
                if (resp != null && resp.IsSuccess && resp.Result != null)
                {
                    var json = JsonSerializer.Serialize(resp.Result);
                    Courses = JsonSerializer.Deserialize<List<CourseResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    ErrorMessage = resp?.ErrorMessage ?? "Failed to load courses";
                }

                var metricsResp = await _courseService.GetInstructorMetricsAsync();
                if (metricsResp.IsSuccess && metricsResp.Result != null)
                {
                    var json = JsonSerializer.Serialize(metricsResp.Result);
                    using var doc = JsonDocument.Parse(json);
                    TotalRevenue = doc.RootElement.GetProperty("TotalRevenue").GetDecimal();
                    TotalStudents = doc.RootElement.GetProperty("TotalStudents").GetInt32();
                    TotalEnrollments = doc.RootElement.GetProperty("TotalEnrollments").GetInt32();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public async Task<IActionResult> OnPostSubmitAsync(Guid courseId)
        {
            TempData["Success"] = "Course submitted successfully! Waiting for Admin approval.";
            return RedirectToPage();
        }
    }
}