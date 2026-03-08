using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize]
    public class LearnModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IClaimService _claimService;
        private readonly IEnrollmentService _enrollmentService;

        public LearnModel(ICourseService courseService, IClaimService claimService, IEnrollmentService enrollmentService)
        {
            _courseService = courseService;
            _claimService = claimService;
            _enrollmentService = enrollmentService;
        }

        public CourseEditSummaryResponse Course { get; set; } = default!;
        public string SyllabusJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            var claim = _claimService.GetUserClaim();

            var enrollCheck = await _enrollmentService.CheckUserEnrollmentAsync(claim.UserId, courseId);
            if (enrollCheck?.IsSuccess != true || !(bool)enrollCheck.Result!)
            {
                TempData["Error"] = "Bạn chưa ghi danh khóa học này!";
                return RedirectToPage("/Courses/Detail", new { id = courseId });
            }

            var response = await _courseService.GetCourseForLearningAsync(courseId);
            if (!response.IsSuccess || response.Result == null)
            {
                var errorText = response.ErrorMessage
                    ?? response.GetType().GetProperty("Message")?.GetValue(response)?.ToString()
                    ?? "Không có thông báo lỗi từ Service";

                var debugInfo = System.Text.Json.JsonSerializer.Serialize(response);

                TempData["Error"] = $"Lỗi thật: {errorText} | Debug: {debugInfo}";

                return RedirectToPage("/Courses/Detail", new { id = courseId });
            }

            var bundle = (CourseEditBundleResponse)response.Result;
            Course = bundle.Course;

            var modules = bundle.Modules ?? new List<CourseModuleEditResponse>();
            var lessons = bundle.Lessons ?? new List<CourseLessonEditResponse>();
            var lessonItems = bundle.LessonItems ?? new List<CourseLessonItemEditResponse>();
            var resources = bundle.LessonResources ?? new List<CourseLessonResourceEditResponse>();
            var gradedItems = bundle.GradedItems ?? new List<CourseGradedItemEditResponse>();
            var questions = bundle.Questions ?? new List<CourseQuestionEditResponse>();
            var answerOptions = bundle.AnswerOptions ?? new List<CourseAnswerOptionEditResponse>();

            var tree = modules.OrderBy(m => m.Index).Select(m => new
            {
                moduleId = m.ModuleId,
                name = m.Name,
                index = m.Index,
                lessons = lessons.Where(l => l.ModuleId == m.ModuleId).OrderBy(l => l.OrderIndex).Select(l => new
                {
                    lessonId = l.LessonId,
                    title = l.Title,
                    orderIndex = l.OrderIndex,
                    items = lessonItems.Where(li => li.LessonId == l.LessonId).OrderBy(li => li.OrderIndex).Select(li =>
                    {
                        var resource = resources.FirstOrDefault(r => r.LessonItemId == li.LessonItemId);
                        var graded = gradedItems.FirstOrDefault(g => g.LessonItemId == li.LessonItemId);

                        var itemQuestions = graded != null ? questions.Where(q => q.GradedItemId == graded.GradedItemId).OrderBy(q => q.OrderIndex).Select(q => new
                        {
                            questionId = q.QuestionId,
                            content = q.Content,
                            points = q.Points,
                            // Tuyệt đối không Select IsCorrect ra frontend
                            options = answerOptions.Where(ao => ao.QuestionId == q.QuestionId).OrderBy(ao => ao.OrderIndex).Select(ao => new { optionId = ao.AnswerOptionId, text = ao.Text })
                        }) : null;

                        return new
                        {
                            itemId = li.LessonItemId,
                            gradedItemId = graded?.GradedItemId,
                            type = li.Type,
                            orderIndex = li.OrderIndex,
                            title = resource?.Title ?? graded?.SubmissionGuidelines ?? "Material",
                            contentUrl = resource?.ResourceUrl,
                            textContent = resource?.TextContent,
                            prompt = itemQuestions?.FirstOrDefault()?.content,
                            questions = itemQuestions
                        };
                    })
                })
            });

            SyllabusJson = JsonSerializer.Serialize(tree, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return Page();
        }


        public async Task<IActionResult> OnPostSubmitWritingAsync(Guid courseId, Guid GradedItemId, string SubmissionText)
        {
            TempData["Success"] = "Đã nộp bài luận! AI đang tiến hành chấm điểm...";
            // TODO: Gọi AI Service ở đây
            return RedirectToPage(new { courseId = courseId });
        }

        public async Task<IActionResult> OnPostSubmitQuizAsync(Guid courseId, Guid GradedItemId)
        {
            TempData["Success"] = "Đã nộp bài trắc nghiệm thành công!";
            // TODO: Gọi Quiz Service tính điểm ở đây
            return RedirectToPage(new { courseId = courseId });
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(Guid courseId, Guid lessonItemId)
        {
            TempData["Success"] = "Đã hoàn thành bài học!";
            // TODO: Gọi Progress Service ở đây
            return RedirectToPage(new { courseId = courseId });
        }
    }
}