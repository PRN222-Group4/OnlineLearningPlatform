using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class CourseReviewModel : PageModel
    {
        private readonly ICourseService _courseService;

        public CourseReviewModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public Course Course { get; set; } = null!;
        public List<Module> Modules { get; set; } = new();
        public List<Lesson> Lessons { get; set; } = new();
        public List<LessonItem> LessonItems { get; set; } = new();
        public List<LessonResource> LessonResources { get; set; } = new();
        public List<GradedItem> GradedItems { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
        public List<AnswerOption> AnswerOptions { get; set; } = new();

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
            Modules = ((IEnumerable<Module>)data.Modules).ToList();
            Lessons = ((IEnumerable<Lesson>)data.Lessons).ToList();
            LessonItems = ((IEnumerable<LessonItem>)data.LessonItems).ToList();
            LessonResources = ((IEnumerable<LessonResource>)data.LessonResources).ToList();
            GradedItems = ((IEnumerable<GradedItem>)data.GradedItems).ToList();
            Questions = ((IEnumerable<Question>)data.Questions).ToList();
            AnswerOptions = ((IEnumerable<AnswerOption>)data.AnswerOptions).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid courseId)
        {
            var request = new ApproveCourseRequest
            {
                CourseId = courseId,
                Status = true // Approve
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
                Status = false, // Reject
                RejectReason = RejectReason
            };
            var result = await _courseService.ApproveCourseAsync(request);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Đã từ chối khóa học" : (result.ErrorMessage ?? "Từ chối khóa học thất bại");
            return RedirectToPage("/Admin/Courses/Pending");
        }
    }
}
