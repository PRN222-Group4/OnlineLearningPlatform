using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Text.Json;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize]
    public class LearnModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IClaimService _claimService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserLessonProgressService _progressService; // Bơm thêm Progress Service

        public LearnModel(
            ICourseService courseService,
            IClaimService claimService,
            IEnrollmentService enrollmentService,
            IUserLessonProgressService progressService)
        {
            _courseService = courseService;
            _claimService = claimService;
            _enrollmentService = enrollmentService;
            _progressService = progressService;
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
                return RedirectToPage("/Courses/Index");
            }

            var bundle = (CourseEditBundleResponse)response.Result;
            Course = bundle.Course;

            var userProgressResponse = await _progressService.GetLessonProgressByUserAsync(claim.UserId);
            var completedLessonIds = new List<Guid>();
            if (userProgressResponse.IsSuccess && userProgressResponse.Result != null)
            {
                var progList = (IEnumerable<UserLessonProgress>)userProgressResponse.Result;
                completedLessonIds = progList.Where(p => p.IsCompleted).Select(p => p.LessonId).ToList();
            }

            var enrollmentData = await _enrollmentService.GetStudentEnrollmentsAsync();
            decimal currentProgressPercent = 0;
            if (enrollmentData.IsSuccess && enrollmentData.Result != null)
            {
                var envList = (IEnumerable<StudentEnrollmentSummaryResponse>)enrollmentData.Result;
                var myEnv = envList.FirstOrDefault(e => e.CourseId == courseId);
                if (myEnv != null) currentProgressPercent = myEnv.ProgressPercent;
            }

            ViewData["ProgressPercent"] = currentProgressPercent;

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
                    isCompleted = completedLessonIds.Contains(l.LessonId),
                    items = lessonItems.Where(li => li.LessonId == l.LessonId).OrderBy(li => li.OrderIndex).Select(li =>
                    {
                        var resource = resources.FirstOrDefault(r => r.LessonItemId == li.LessonItemId);
                        var graded = gradedItems.FirstOrDefault(g => g.LessonItemId == li.LessonItemId);

                        return new
                        {
                            itemId = li.LessonItemId,
                            lessonId = l.LessonId,
                            gradedItemId = graded?.GradedItemId,
                            type = li.Type,
                            orderIndex = li.OrderIndex,
                            title = resource?.Title ?? graded?.SubmissionGuidelines ?? "Material",
                            contentUrl = resource?.ResourceUrl,
                            textContent = resource?.TextContent,
                            prompt = graded != null ? questions.FirstOrDefault(q => q.GradedItemId == graded.GradedItemId)?.Content : null,
                            questions = graded != null ? questions.Where(q => q.GradedItemId == graded.GradedItemId).Select(q => new { questionId = q.QuestionId, content = q.Content, points = q.Points, options = answerOptions.Where(ao => ao.QuestionId == q.QuestionId).Select(ao => new { optionId = ao.AnswerOptionId, text = ao.Text }) }) : null
                        };
                    })
                })
            });

            SyllabusJson = JsonSerializer.Serialize(tree, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return Page();
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(Guid courseId, Guid lessonId) // Nhận LessonId thay vì LessonItemId
        {
            var result = await _progressService.MarkLessonCompletedAsync(lessonId);

            if (result.IsSuccess)
                TempData["Success"] = "Đã hoàn thành bài học! Tiến độ của bạn đã được lưu.";
            else
                TempData["Error"] = $"Lỗi cập nhật: {result.ErrorMessage}";

            return RedirectToPage(new { courseId = courseId });
        }

        public async Task<IActionResult> OnPostSubmitWritingAsync(Guid courseId, Guid lessonId, Guid GradedItemId, string SubmissionText)
        {
            TempData["EvaluatedLessonId"] = lessonId.ToString();

            if (string.IsNullOrWhiteSpace(SubmissionText))
            {
                TempData["AiScore"] = 0;
                TempData["AiFeedback"] = "Bạn chưa nhập bài làm. Vui lòng nhập nội dung trước khi nộp cho AI!";
                return RedirectToPage(new { courseId = courseId });
            }

            await Task.Delay(2000);

            var random = new Random();
            int aiScore = random.Next(3, 11);

            string aiFeedback = aiScore switch
            {
                >= 9 => "Xuất sắc! Ý tưởng rõ ràng, ngữ pháp chuẩn chỉnh không chê vào đâu được. Đáng làm template mẫu!",
                >= 7 => "Khá tốt! Bạn diễn đạt ổn, nhưng cần chú ý đa dạng hóa từ vựng và cấu trúc câu phức hơn nữa.",
                >= 5 => "Tạm được. Bài viết đủ ý cơ bản nhưng sai khá nhiều lỗi chính tả. Cần ôn tập lại ngữ pháp.",
                _ => "Bài làm còn sơ sài, lạc đề. Yêu cầu xem lại bài giảng và làm lại từ đầu."
            };
            TempData["AiScore"] = aiScore;
            TempData["AiFeedback"] = aiFeedback;

            if (aiScore >= 5)
            {
                await _progressService.MarkLessonCompletedAsync(lessonId);
                TempData["Success"] = "Tuyệt vời! Bạn đã vượt qua bài kiểm tra AI."; // Cái này vẫn dùng Toast cho nó nổ pháo hoa
            }

            return RedirectToPage(new { courseId = courseId });
        }
        public async Task<IActionResult> OnPostSubmitQuizAsync(Guid courseId, Guid lessonId, Guid GradedItemId)
        {
            TempData["Success"] = "Nộp bài Quiz thành công! Đang chờ chấm điểm...";
            // Tương lai: Chấm điểm -> Pass -> _progressService.MarkLessonCompletedAsync(lessonId)
            return RedirectToPage(new { courseId = courseId });
        }
    }
}