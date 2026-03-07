using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize]
    public class MyCoursesModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;

        public MyCoursesModel(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        public List<StudentEnrollmentSummaryResponse> Enrollments { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var response = await _enrollmentService.GetStudentEnrollmentsAsync();
            if (!response.IsSuccess)
            {
                ErrorMessage = response.ErrorMessage ?? "Failed to load enrolled courses.";
                return;
            }

            Enrollments = (response.Result as IEnumerable<StudentEnrollmentSummaryResponse>)?.ToList()
                ?? new List<StudentEnrollmentSummaryResponse>();
        }
    }
}
