using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize]
    public class LearningModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IUserLessonProgressService _progressService;

        public LearningModel(ICourseService courseService, IUserLessonProgressService progressService)
        {
            _courseService = courseService;
            _progressService = progressService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid CourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? LessonId { get; set; }

        public StudentLearningDetailResponse? CourseDetail { get; set; }
        public StudentLearningLessonResponse? CurrentLesson { get; set; }
        public Guid? NextLessonId { get; set; }
        public Guid? PreviousLessonId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _courseService.GetCourseDetailForStudentAsync(CourseId);
            if (!response.IsSuccess || response.Result == null)
            {
                ErrorMessage = response.ErrorMessage ?? "Unable to load course learning data.";
                return Page();
            }

            CourseDetail = response.Result as StudentLearningDetailResponse;
            if (CourseDetail == null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response.Result);
                CourseDetail = System.Text.Json.JsonSerializer.Deserialize<StudentLearningDetailResponse>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (CourseDetail == null)
            {
                ErrorMessage = "Unable to read course learning data.";
                return Page();
            }

            var orderedLessons = CourseDetail.Modules
                .OrderBy(m => m.OrderIndex)
                .SelectMany(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .ToList();

            if (!orderedLessons.Any())
            {
                ErrorMessage = "This course does not have any lesson yet.";
                return Page();
            }

            if (!LessonId.HasValue)
            {
                return RedirectToPage(new { courseId = CourseId, lessonId = orderedLessons.First().LessonId });
            }

            CurrentLesson = orderedLessons.FirstOrDefault(l => l.LessonId == LessonId.Value);
            if (CurrentLesson == null)
            {
                return RedirectToPage(new { courseId = CourseId, lessonId = orderedLessons.First().LessonId });
            }

            var currentIndex = orderedLessons.FindIndex(l => l.LessonId == CurrentLesson.LessonId);
            if (currentIndex > 0)
            {
                PreviousLessonId = orderedLessons[currentIndex - 1].LessonId;
            }

            if (currentIndex >= 0 && currentIndex < orderedLessons.Count - 1)
            {
                NextLessonId = orderedLessons[currentIndex + 1].LessonId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(Guid lessonId)
        {
            await _progressService.MarkLessonCompletedAsync(lessonId);

            // Check nếu course đã 100% thì redirect sang MyCertificates
            var response = await _courseService.GetCourseDetailForStudentAsync(CourseId);
            if (response.IsSuccess && response.Result != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response.Result);
                var detail = System.Text.Json.JsonSerializer.Deserialize<StudentLearningDetailResponse>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (detail?.ProgressPercent >= 100)
                    return RedirectToPage("/Student/MyCertificates");
            }

            return RedirectToPage(new { courseId = CourseId, lessonId });
        }
    }
}
