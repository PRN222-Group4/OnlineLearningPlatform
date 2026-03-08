using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class PreviewModel : PageModel
    {
        private readonly ICourseService _courseService;

        public PreviewModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public CourseEditSummaryResponse Course { get; set; } = default!;

        public string SyllabusJson { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            var response = await _courseService.GetCourseForEditAsync(courseId);
            if (!response.IsSuccess || response.Result == null)
            {
                TempData["Error"] = "Không thể tải dữ liệu khóa học.";
                return RedirectToPage("/Teacher/Dashboard");
            }

            var bundle = (CourseEditBundleResponse)response.Result;
            Course = bundle.Course;

            var tree = bundle.Modules.OrderBy(m => m.Index).Select(m => new
            {
                moduleId = m.ModuleId,
                name = m.Name,
                index = m.Index,
                lessons = bundle.Lessons.Where(l => l.ModuleId == m.ModuleId).OrderBy(l => l.OrderIndex).Select(l => new
                {
                    lessonId = l.LessonId,
                    title = l.Title,
                    orderIndex = l.OrderIndex,
                    items = bundle.LessonItems.Where(li => li.LessonId == l.LessonId).OrderBy(li => li.OrderIndex).Select(li =>
                    {
                        var resource = bundle.LessonResources.FirstOrDefault(r => r.LessonItemId == li.LessonItemId);
                        var graded = bundle.GradedItems.FirstOrDefault(g => g.LessonItemId == li.LessonItemId);
                        var itemQuestions = graded != null ? bundle.Questions.Where(q => q.GradedItemId == graded.GradedItemId).OrderBy(q => q.OrderIndex).Select(q => new
                        {
                            content = q.Content,
                            points = q.Points,
                            options = bundle.AnswerOptions.Where(ao => ao.QuestionId == q.QuestionId).OrderBy(ao => ao.OrderIndex).Select(ao => new { text = ao.Text, isCorrect = ao.IsCorrect })
                        }) : null;

                        return new
                        {
                            itemId = li.LessonItemId,
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

        public async Task<IActionResult> OnPostSubmitCourseAsync(Guid courseId)
        {
            var response = await _courseService.ValidateAndSubmitForReviewAsync(courseId);
            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage;
                return RedirectToPage(new { courseId });
            }

            TempData["Success"] = "Khóa học đã được nộp thành công! Vui lòng chờ Admin phê duyệt.";
            return RedirectToPage("/Teacher/Dashboard"); 
        }
    }
}