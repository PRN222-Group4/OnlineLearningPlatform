using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;

        public CreateModel(ICourseService courseService, IModuleService moduleService)
        {
            _courseService = courseService;
            _moduleService = moduleService;
        }

        [BindProperty]
        public CreateNewCourseRequest Input { get; set; } = new();

        // For editing existing draft course
        public CourseEditSummaryResponse? ExistingCourse { get; set; }
        public Guid? CourseId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? courseId)
        {
            if (courseId.HasValue)
            {
                CourseId = courseId;
                var result = await _courseService.GetCourseForEditAsync(courseId.Value);
                if (result.IsSuccess && result.Result != null)
                {
                    var data = (CourseEditBundleResponse)result.Result;
                    ExistingCourse = data.Course;
                    if (ExistingCourse != null && ExistingCourse.Status != 0)
                    {
                        TempData["Error"] = "Chỉ có thể chỉnh sửa khóa học ở trạng thái Draft";
                        return RedirectToPage("/Teacher/Dashboard");
                    }
                    // Pre-populate the form
                    Input = new CreateNewCourseRequest
                    {
                        Title = ExistingCourse!.Title,
                        Subtitle = ExistingCourse.Subtitle,
                        Description = ExistingCourse.Description,
                        Price = ExistingCourse.Price,
                        Level = ExistingCourse.Level,
                        LanguageId = ExistingCourse.LanguageId,
                        Tags = ExistingCourse.Tags
                    };
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy khóa học";
                    return RedirectToPage("/Teacher/Dashboard");
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? courseId)
        {
            if (!ModelState.IsValid)
            {
                CourseId = courseId;
                return Page();
            }

            if (courseId.HasValue)
            {
                // Update existing draft course
                var updateRequest = new UpdateCourseRequest
                {
                    CourseId = courseId.Value,
                    Title = Input.Title,
                    Subtitle = Input.Subtitle,
                    Description = Input.Description,
                    Price = Input.Price,
                    Level = Input.Level,
                    LanguageId = Input.LanguageId,
                    Tags = Input.Tags,
                    ImageFile = Input.ImageFile
                };
                var result = await _courseService.UpdateCourseAsync(updateRequest);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.ErrorMessage ?? "Cập nhật thất bại");
                    CourseId = courseId;
                    return Page();
                }
                return RedirectToPage("/Teacher/Courses/EditLessons", new { courseId = courseId.Value });
            }
            else
            {
                // Create new course
                var result = await _courseService.CreateNewCourseAsync(Input);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.ErrorMessage ?? "Tạo khóa học thất bại");
                    return Page();
                }
                var newCourseId = (Guid)result.Result!;
                return RedirectToPage("/Teacher/Courses/EditLessons", new { courseId = newCourseId });
            }
        }
    }
}
