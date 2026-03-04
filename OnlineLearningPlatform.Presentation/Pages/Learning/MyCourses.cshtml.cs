using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.Presentation.Pages.Learning
{
    [Authorize]
    public class MyCoursesModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        public MyCoursesModel(IEnrollmentService enrollmentService)
            => _enrollmentService = enrollmentService;

        public List<Enrollment> Enrollments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var resp = await _enrollmentService.GetStudentEnrollmentsAsync();
            if (resp?.IsSuccess == true && resp.Result is List<Enrollment> list)
                Enrollments = list;
            return Page();
        }
    }
}