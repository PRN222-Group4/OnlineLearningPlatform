using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Collections;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        public CreateModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty]
        public CreateNewCourseRequest Input { get; set; } = new();

        public CourseEditSummaryResponse? ExistingCourse { get; set; }
        public Guid? CourseId { get; set; }

        public SelectList LanguageOptions { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? courseId)
        {
            await LoadLanguagesAsync();

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
                await LoadLanguagesAsync();
                CourseId = courseId;
                return Page();
            }

            if (courseId.HasValue)
            {
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
                    await LoadLanguagesAsync();
                    CourseId = courseId;
                    return Page();
                }
                return RedirectToPage("/Teacher/Courses/EditModules", new { courseId = courseId.Value });
            }
            else
            {
                var result = await _courseService.CreateNewCourseAsync(Input);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.ErrorMessage ?? "Tạo khóa học thất bại");
                    await LoadLanguagesAsync();
                    return Page();
                }
                var newCourseId = (Guid)result.Result!;
                return RedirectToPage("/Teacher/Courses/EditModules", new { courseId = newCourseId });
            }
        }

        private async Task LoadLanguagesAsync()
        {
            var response = await _courseService.GetActiveLanguagesAsync();
            if (response.IsSuccess && response.Result != null)
            {
                LanguageOptions = new SelectList((IEnumerable)response.Result, "LanguageId", "Name");
            }
            else
            {
                LanguageOptions = new SelectList(new List<SelectListItem>());
            }
        }
    }
}