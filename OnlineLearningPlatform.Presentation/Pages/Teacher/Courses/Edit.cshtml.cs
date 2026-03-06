using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class EditModel : PageModel
    {
        private readonly ICourseService _courseService;

        public EditModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid courseId, int? step)
        {
            if (step.HasValue)
            {
                if (step.Value == 1) return RedirectToPage("/Teacher/Courses/Create", new { courseId });
                if (step.Value == 2) return RedirectToPage("/Teacher/Courses/EditLessons", new { courseId });
                if (step.Value == 3) return RedirectToPage("/Teacher/Courses/EditMaterials", new { courseId });
            }

            var result = await _courseService.GetCourseForEditAsync(courseId);
            if (!result.IsSuccess || result.Result == null)
            {
                TempData["Error"] = result.ErrorMessage ?? "Không tìm thấy khóa học";
                return RedirectToPage("/Teacher/Dashboard");
            }

            CourseId = courseId;
            var data = (CourseEditBundleResponse)result.Result;
            Course = data.Course;

            if (Course.Status != 0)
            {
                TempData["Error"] = "Khóa học không còn ở trạng thái Draft. Bạn chỉ có thể xem chi tiết.";
            }

            return Page();
        }
    }
}
