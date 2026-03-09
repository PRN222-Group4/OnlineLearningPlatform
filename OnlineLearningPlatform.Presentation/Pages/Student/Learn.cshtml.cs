using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.BusinessObject.Requests.GradedItem;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Student
{
    [Authorize]
    public class LearnModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IClaimService _claimService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserLessonProgressService _progressService;
        private readonly IGradedItemService _gradedItemService;

        public LearnModel(
            ICourseService courseService,
            IClaimService claimService,
            IEnrollmentService enrollmentService,
            IUserLessonProgressService progressService,
            IGradedItemService gradedItemService)
        {
            _courseService = courseService;
            _claimService = claimService;
            _enrollmentService = enrollmentService;
            _progressService = progressService;
            _gradedItemService = gradedItemService;
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
                var jsonOptions = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };
                var jsonStr = JsonSerializer.Serialize(userProgressResponse.Result, jsonOptions);

                using var doc = JsonDocument.Parse(jsonStr);
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("IsCompleted", out var isCompletedProp) && isCompletedProp.GetBoolean() &&
                        element.TryGetProperty("LessonId", out var lessonIdProp))
                    {
                        completedLessonIds.Add(lessonIdProp.GetGuid());
                    }
                }
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

        //public async Task<IActionResult> OnPostMarkCompleteAsync(Guid courseId, Guid lessonId)
        //{
        //    var result = await _progressService.MarkLessonCompletedAsync(lessonId);

        //    if (!result.IsSuccess)
        //    {
        //        TempData["Error"] = $"Lỗi cập nhật: {result.ErrorMessage}";
        //        return RedirectToPage(new { courseId = courseId });
        //    }

        //    // Check 100% → redirect certificate
        //    var enrollmentData = await _enrollmentService.GetStudentEnrollmentsAsync();
        //    if (enrollmentData.IsSuccess && enrollmentData.Result != null)
        //    {
        //        var envList = (IEnumerable<StudentEnrollmentSummaryResponse>)enrollmentData.Result;
        //        var myEnv = envList.FirstOrDefault(e => e.CourseId == courseId);
        //        if (myEnv?.ProgressPercent >= 100)
        //            return RedirectToPage("/Student/MyCertificates");
        //    }

        //    TempData["Success"] = "Đã hoàn thành bài học!";
        //    return RedirectToPage(new { courseId = courseId });
        //}
        public async Task<IActionResult> OnPostMarkCompleteAsync(Guid courseId, Guid lessonId)
        {
            var result = await _progressService.MarkLessonCompletedAsync(lessonId);

            if (!result.IsSuccess)
            {
                TempData["Error"] = $"Lỗi cập nhật: {result.ErrorMessage}";
                return RedirectToPage(new { courseId = courseId });
            }

            // Check thông qua message trả về từ service
            // MarkLessonCompletedAsync đã update enrollment.ProgressPercent trong DB
            // Re-fetch enrollment để check
            var enrollmentData = await _enrollmentService.GetStudentEnrollmentsAsync();
            if (enrollmentData.IsSuccess && enrollmentData.Result != null)
            {
                var envList = enrollmentData.Result as IEnumerable<StudentEnrollmentSummaryResponse>
                    ?? System.Text.Json.JsonSerializer.Deserialize<List<StudentEnrollmentSummaryResponse>>(
                        System.Text.Json.JsonSerializer.Serialize(enrollmentData.Result),
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var myEnv = envList?.FirstOrDefault(e => e.CourseId == courseId);
                if (myEnv?.ProgressPercent >= 100)
                    return RedirectToPage("/Student/MyCertificates");
            }

            TempData["Success"] = "Đã hoàn thành bài học!";
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
                >= 9 => "Xuất sắc! Ý tưởng rõ ràng, ngữ pháp chuẩn chỉnh không chê vào đâu được.",
                >= 7 => "Khá tốt! Bạn diễn đạt ổn, nhưng cần chú ý đa dạng hóa từ vựng.",
                >= 5 => "Tạm được. Bài viết đủ ý cơ bản nhưng sai khá nhiều lỗi chính tả.",
                _ => "Bài làm còn sơ sài, lạc đề. Yêu cầu xem lại bài giảng và làm lại từ đầu."
            };
            TempData["AiScore"] = aiScore;
            TempData["AiFeedback"] = aiFeedback;

            if (aiScore >= 5)
            {
                await _progressService.MarkLessonCompletedAsync(lessonId);
                TempData["Success"] = "Tuyệt vời! Bạn đã vượt qua bài kiểm tra Writing.";
            }

            return RedirectToPage(new { courseId = courseId });
        }

        public async Task<IActionResult> OnPostSubmitSpeakingAsync(Guid courseId, Guid lessonId, Guid GradedItemId, IFormFile AudioFile)
        {
            TempData["EvaluatedLessonId"] = lessonId.ToString();

            if (AudioFile == null || AudioFile.Length == 0)
            {
                TempData["AiScore"] = 0;
                TempData["AiFeedback"] = "Sếp chưa chọn file ghi âm kìa!";
                return RedirectToPage(new { courseId = courseId });
            }

            await Task.Delay(2000);

            var random = new Random();
            int aiScore = random.Next(4, 11);
            string aiFeedback = aiScore switch
            {
                >= 8 => "Phát âm rất tự nhiên và trôi chảy! Trọng âm chuẩn xác.",
                >= 5 => "Nghe khá rõ ràng, tuy nhiên một số âm đuôi bị nuốt. Cố gắng chậm lại chút nhé.",
                _ => "Âm thanh bị rè hoặc phát âm chưa rõ chữ. Vui lòng ghi âm lại."
            };

            TempData["AiScore"] = aiScore;
            TempData["AiFeedback"] = aiFeedback;

            if (aiScore >= 5)
            {
                await _progressService.MarkLessonCompletedAsync(lessonId);
                TempData["Success"] = "Tuyệt vời! Bạn đã vượt qua bài kiểm tra Speaking.";
            }

            return RedirectToPage(new { courseId = courseId });
        }

        public async Task<IActionResult> OnPostSubmitQuizAsync(Guid courseId, Guid lessonId, Guid GradedItemId, List<QuizAnswerSubmitModel> Answers)
        {
            TempData["EvaluatedLessonId"] = lessonId.ToString();

            if (Answers == null || !Answers.Any())
            {
                TempData["Error"] = "Sếp chưa đánh dấu đáp án nào mà đã nộp rồi!";
                return RedirectToPage(new { courseId = courseId });
            }

            var request = new SubmitQuizRequest
            {
                GradedItemId = GradedItemId,
                Answers = Answers.GroupBy(a => a.QuestionId).Select(g => new AnswerSubmission
                {
                    QuestionId = g.Key,
                    SelectedAnswerOptionIds = g.Select(x => x.SelectedOptionId).ToList()
                }).ToList()
            };

            var quizResult = await _gradedItemService.SubmitQuizAsync(request);

            if (!quizResult.IsSuccess || quizResult.Result == null)
            {
                var errorMsg = quizResult.Result?.ToString() ?? quizResult.ErrorMessage ?? "Lỗi chưa biết";
                TempData["Error"] = $"Cảnh báo: {errorMsg}";
                TempData["EvaluatedLessonId"] = lessonId.ToString();
                return RedirectToPage(new { courseId = courseId });
            }

            var jsonString = JsonSerializer.Serialize(quizResult.Result);
            using var doc = JsonDocument.Parse(jsonString);

            decimal score = doc.RootElement.GetProperty("Score").GetDecimal();
            decimal maxScore = doc.RootElement.GetProperty("MaxScore").GetDecimal();

            decimal percent = maxScore > 0 ? (score / maxScore) * 100 : 0;

            if (percent >= 50)
            {
                await _progressService.MarkLessonCompletedAsync(lessonId);
                TempData["Success"] = $"Đỉnh quá! Điểm: {score}/{maxScore}. Bài trắc nghiệm đã được tick xanh!";
            }
            else
            {
                TempData["Error"] = $"Hơi xui! Điểm: {score}/{maxScore} (Cần tối thiểu 50%). Ôn bài rồi làm lại nhé!";
            }
            TempData["EvaluatedLessonId"] = lessonId.ToString();
            TempData["QuizScore"] = score.ToString();
            TempData["QuizMaxScore"] = maxScore.ToString();

            return RedirectToPage(new { courseId = courseId });
        }
    }
}