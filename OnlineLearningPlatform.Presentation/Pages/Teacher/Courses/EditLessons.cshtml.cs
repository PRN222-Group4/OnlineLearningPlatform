using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Module;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class EditLessonsModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly IModuleService _moduleService;

        public EditLessonsModel(ICourseService courseService, ILessonService lessonService, IModuleService moduleService)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _moduleService = moduleService;
        }

        public CourseEditSummaryResponse Course { get; set; } = new();
        public List<CourseModuleEditResponse> Modules { get; set; } = new();
        public List<CourseLessonEditResponse> Lessons { get; set; } = new();
        public Guid CourseId { get; set; }
        public bool IsReadOnly => Course?.Status != 0;

        [BindProperty]
        public string LessonTitle { get; set; } = "";

        [BindProperty]
        public int EstimatedMinutes { get; set; } = 15;

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            CourseId = courseId;
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            return Page();
        }

        public async Task<IActionResult> OnPostAddLessonAsync(Guid courseId)
        {
            CourseId = courseId;
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            if (string.IsNullOrWhiteSpace(LessonTitle))
            {
                TempData["Error"] = "Tên bài học không được để trống";
                return Page();
            }

            // Get first module
            var modulesResult = await _moduleService.GetModulesByCourseAsync(courseId);
            if (!modulesResult.IsSuccess || modulesResult.Result == null)
            {
                TempData["Error"] = "Không tìm thấy module";
                return Page();
            }
            var modules = (IEnumerable<ModuleResponse>)modulesResult.Result;
            var firstModule = modules.FirstOrDefault();
            if (firstModule == null)
            {
                TempData["Error"] = "Khóa học chưa có module";
                return Page();
            }

            var request = new CreateNewLessonForModuleRequest
            {
                ModuleId = firstModule.ModuleId,
                Title = LessonTitle,
                EstimatedMinutes = EstimatedMinutes,
                OrderIndex = Lessons.Count
            };

            var result = await _lessonService.CreateNewLessonForModuleAsync(request);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage ?? "Tạo bài học thất bại";
                return Page();
            }

            TempData["Success"] = "Đã thêm bài học thành công!";
            return RedirectToPage(new { courseId });
        }

        public async Task<IActionResult> OnPostDeleteLessonAsync(Guid courseId, Guid lessonId)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            var result = await _lessonService.DeleteLessonAsync(lessonId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage ?? "Xóa bài học thất bại";
            }
            else
            {
                TempData["Success"] = "Đã xóa bài học";
            }
            return RedirectToPage(new { courseId });
        }

        private async Task<bool> LoadCourseData(Guid courseId)
        {
            var result = await _courseService.GetCourseForEditAsync(courseId);
            if (!result.IsSuccess || result.Result == null) return false;

            var data = (CourseEditBundleResponse)result.Result;
            Course = data.Course;
            Modules = data.Modules.ToList();
            Lessons = data.Lessons.ToList();
            return true;
        }
    }
}
