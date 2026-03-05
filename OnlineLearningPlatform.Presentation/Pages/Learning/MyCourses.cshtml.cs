using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;

namespace OnlineLearningPlatform.Presentation.Pages.Learning
{
    public class MyCoursesModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;

        public MyCoursesModel(IEnrollmentService enrollmentService, ICourseService courseService)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
        }

        public List<EnrolledCourseViewModel> Enrollments { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return RedirectToPage("/Account/Login");

            try
            {
                var resp = await _enrollmentService.GetStudentEnrollmentsAsync();
                if (resp != null && resp.IsSuccess && resp.Result != null)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(resp.Result);
                    var enrollments = System.Text.Json.JsonSerializer.Deserialize<List<EnrollmentDto>>(json,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (enrollments != null)
                    {
                        Enrollments = enrollments.Select(e => new EnrolledCourseViewModel
                        {
                            CourseId = e.CourseId,
                            CourseTitle = e.Course?.Title ?? "Untitled",
                            CourseImage = e.Course?.Image ?? "",
                            ProgressPercent = e.ProgressPercent,
                            EnrolledAt = e.EnrolledAt
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return Page();
        }
    }

    public class EnrolledCourseViewModel
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string CourseImage { get; set; } = "";
        public int ProgressPercent { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    public class EnrollmentDto
    {
        public Guid EnrollmentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public int ProgressPercent { get; set; }
        public int Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public CourseInfoDto? Course { get; set; }
    }

    public class CourseInfoDto
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public decimal Price { get; set; }
    }
}
