using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Admin.Courses
{
    public class ReviewModel : PageModel
    {
        private readonly ICourseService _courseService;

        public ReviewModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public CourseEditSummaryResponse Course { get; set; } = new();
        public List<CourseModuleEditResponse> Modules { get; set; } = new();
        public List<CourseLessonEditResponse> Lessons { get; set; } = new();
        public List<CourseLessonItemEditResponse> LessonItems { get; set; } = new();
        public List<CourseLessonResourceEditResponse> LessonResources { get; set; } = new();
        public List<CourseGradedItemEditResponse> GradedItems { get; set; } = new();
        public List<CourseQuestionEditResponse> Questions { get; set; } = new();
        public List<CourseAnswerOptionEditResponse> AnswerOptions { get; set; } = new();

        [BindProperty]
        public string? RejectReason { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            var editResult = await _courseService.GetCourseForEditAsync(courseId);
            if (!editResult.IsSuccess || editResult.Result == null)
            {
                return RedirectToPage("/Admin/Courses/Pending");
            }

            var data = (CourseEditBundleResponse)editResult.Result;
            Course = data.Course;
            if (Course.Status != 1) return RedirectToPage("/Admin/Courses/Pending");
            Modules = data.Modules.ToList();
            Lessons = data.Lessons.ToList();
            LessonItems = data.LessonItems.ToList();
            LessonResources = data.LessonResources.ToList();
            GradedItems = data.GradedItems.ToList();
            Questions = data.Questions.ToList();
            AnswerOptions = data.AnswerOptions.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid courseId)
        {
            var request = new ApproveCourseRequest
            {
                CourseId = courseId,
                Status = true
            };
            var result = await _courseService.ApproveCourseAsync(request);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Duyệt khóa học thành công" : (result.ErrorMessage ?? "Duyệt khóa học thất bại");
            return RedirectToPage("/Admin/Courses/Pending");
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid courseId)
        {
            if (string.IsNullOrWhiteSpace(RejectReason))
            {
                TempData["Error"] = "Lý do từ chối là bắt buộc";
                return RedirectToPage(new { courseId });
            }

            var request = new ApproveCourseRequest
            {
                CourseId = courseId,
                Status = false,
                RejectReason = RejectReason
            };
            var result = await _courseService.ApproveCourseAsync(request);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Đã từ chối khóa học" : (result.ErrorMessage ?? "Từ chối khóa học thất bại");
            return RedirectToPage("/Admin/Courses/Pending");
        }
    }
}
