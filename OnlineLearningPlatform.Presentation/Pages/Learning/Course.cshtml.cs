using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;

namespace OnlineLearningPlatform.Presentation.Pages.Learning
{
    public class CourseModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserLessonProgressService _progressService;

        public CourseModel(
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            IUserLessonProgressService progressService)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _progressService = progressService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid CourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? LessonId { get; set; }

        public CourseDetailDto? CourseDetail { get; set; }
        public List<ModuleDto>? Modules { get; set; }
        public LessonDto? CurrentLesson { get; set; }
        public Guid? CurrentLessonId => LessonId;
        public Guid? NextLessonId { get; set; }
        public Guid? PreviousLessonId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Check enrollment
                var isEnrolled = await _enrollmentService.CheckEnrollmentAsync(CourseId);
                if (!isEnrolled)
                {
                    ErrorMessage = "You are not enrolled in this course.";
                    return Page();
                }

                // Get course details
                var courseResp = await _courseService.GetCourseDetailForStudentAsync(CourseId);
                if (courseResp == null || !courseResp.IsSuccess || courseResp.Result == null)
                {
                    ErrorMessage = courseResp?.ErrorMessage ?? "Course not found";
                    return Page();
                }

                // Map course detail (simplified - you may need to adjust based on actual response structure)
                var json = System.Text.Json.JsonSerializer.Serialize(courseResp.Result);
                var detail = System.Text.Json.JsonSerializer.Deserialize<CourseDetailDto>(json, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                CourseDetail = detail;
                Modules = detail?.Modules ?? new List<ModuleDto>();

                // Find current lesson
                if (LessonId.HasValue && Modules != null)
                {
                    foreach (var module in Modules)
                    {
                        var lesson = module.Lessons?.FirstOrDefault(l => l.LessonId == LessonId.Value);
                        if (lesson != null)
                        {
                            CurrentLesson = lesson;
                            break;
                        }
                    }

                    // Find next/previous lessons
                    var allLessons = Modules.SelectMany(m => m.Lessons ?? new List<LessonDto>()).ToList();
                    var currentIndex = allLessons.FindIndex(l => l.LessonId == LessonId.Value);
                    if (currentIndex >= 0)
                    {
                        if (currentIndex > 0)
                            PreviousLessonId = allLessons[currentIndex - 1].LessonId;
                        if (currentIndex < allLessons.Count - 1)
                            NextLessonId = allLessons[currentIndex + 1].LessonId;
                    }
                }
                else if (Modules != null && Modules.Any())
                {
                    // Default to first lesson
                    var firstLesson = Modules.FirstOrDefault()?.Lessons?.FirstOrDefault();
                    if (firstLesson != null)
                    {
                        return RedirectToPage(new { courseId = CourseId, lessonId = firstLesson.LessonId });
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(Guid lessonId)
        {
            try
            {
                await _progressService.MarkLessonCompletedAsync(lessonId);
                return RedirectToPage(new { courseId = CourseId, lessonId = lessonId });
            }
            catch
            {
                return RedirectToPage(new { courseId = CourseId, lessonId = lessonId });
            }
        }
    }

    // DTOs for view model
    public class CourseDetailDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProgressPercent { get; set; }
        public List<ModuleDto>? Modules { get; set; }
    }

    public class ModuleDto
    {
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int LessonCount { get; set; }
        public List<LessonDto>? Lessons { get; set; }
    }

    public class LessonDto
    {
        public Guid LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public List<LessonItemDto>? Items { get; set; }
    }

    public class LessonItemDto
    {
        public Guid LessonItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Type { get; set; } // 0=Video, 1=Reading, etc.
        public string? VideoUrl { get; set; }
        public string? Content { get; set; }
    }
}
