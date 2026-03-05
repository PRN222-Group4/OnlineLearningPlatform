using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Responses.Module;
using OnlineLearningPlatform.BusinessObject.Responses.Lesson;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher
{
    public class CourseManageModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IModuleService _moduleService;
        private readonly ILessonService _lessonService;

        public CourseManageModel(ICourseService courseService, IModuleService moduleService, ILessonService lessonService)
        {
            _courseService = courseService;
            _moduleService = moduleService;
            _lessonService = lessonService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid CourseId { get; set; }

        public string CourseTitle { get; set; } = "";
        public string? CourseDescription { get; set; }
        public int CourseStatus { get; set; }
        public List<ModuleViewModel> Modules { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public async Task OnGetAsync()
        {
            await LoadCourseDataAsync();
        }

        public async Task<IActionResult> OnPostAddModuleAsync(Guid courseId, string moduleTitle, string? moduleDescription)
        {
            CourseId = courseId;
            try
            {
                await _moduleService.CreateNewModuleForCourseAsync(new CreateNewModuleForCourseRequest
                {
                    CourseId = courseId,
                    Name = moduleTitle,
                    Description = moduleDescription ?? ""
                });
                SuccessMessage = "Module created successfully!";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }

            await LoadCourseDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteModuleAsync(Guid moduleId, Guid courseId)
        {
            CourseId = courseId;
            try
            {
                await _moduleService.DeleteModuleAsync(moduleId);
                SuccessMessage = "Module deleted.";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }

            await LoadCourseDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddLessonAsync(Guid moduleId, Guid courseId, string lessonTitle, int lessonType)
        {
            CourseId = courseId;
            try
            {
                await _lessonService.CreateNewLessonForModuleAsync(new CreateNewLessonForModuleRequest
                {
                    ModuleId = moduleId,
                    Title = lessonTitle,
                    Content = ""
                });
                SuccessMessage = "Lesson added successfully!";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }

            await LoadCourseDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteLessonAsync(Guid lessonId, Guid courseId)
        {
            CourseId = courseId;
            try
            {
                await _lessonService.DeleteLessonAsync(lessonId);
                SuccessMessage = "Lesson deleted.";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }

            await LoadCourseDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitForReviewAsync(Guid courseId)
        {
            CourseId = courseId;
            try
            {
                await _courseService.SubmitCourseForReviewAsync(courseId);
                SuccessMessage = "Course submitted for admin review!";
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }

            await LoadCourseDataAsync();
            return Page();
        }

        private async Task LoadCourseDataAsync()
        {
            try
            {
                // Load course info
                var courseResp = await _courseService.GetCourseByIdAsync(CourseId);
                if (courseResp?.IsSuccess == true && courseResp.Result != null)
                {
                    var json = JsonSerializer.Serialize(courseResp.Result);
                    var course = JsonSerializer.Deserialize<CourseDto>(json, _jsonOpts);
                    if (course != null)
                    {
                        CourseTitle = course.Title ?? "";
                        CourseDescription = course.Description;
                        CourseStatus = course.Status;
                    }
                }

                // Load modules with lessons
                var modulesResp = await _moduleService.GetModulesByCourseAsync(CourseId);
                if (modulesResp?.IsSuccess == true && modulesResp.Result != null)
                {
                    var json = JsonSerializer.Serialize(modulesResp.Result);
                    var modules = JsonSerializer.Deserialize<List<ModuleViewModel>>(json, _jsonOpts);
                    Modules = modules ?? new List<ModuleViewModel>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }

    // View models for CourseManage
    public class CourseDto
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public string? Image { get; set; }
    }

    public class ModuleViewModel
    {
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Index { get; set; }
        public List<LessonViewModel>? Lessons { get; set; }
    }

    public class LessonViewModel
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int Type { get; set; }
        public int OrderIndex { get; set; }
        public string? Duration { get; set; }
    }
}
