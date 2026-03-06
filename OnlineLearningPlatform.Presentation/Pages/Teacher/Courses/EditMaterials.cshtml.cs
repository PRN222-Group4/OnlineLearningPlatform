using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.LessonItem;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class EditMaterialsModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ILessonItemService _lessonItemService;

        public EditMaterialsModel(ICourseService courseService, ILessonItemService lessonItemService)
        {
            _courseService = courseService;
            _lessonItemService = lessonItemService;
        }

        public Course Course { get; set; } = null!;
        public List<Module> Modules { get; set; } = new();
        public List<Lesson> Lessons { get; set; } = new();
        public List<LessonItem> LessonItems { get; set; } = new();
        public List<LessonResource> LessonResources { get; set; } = new();
        public List<GradedItem> GradedItems { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
        public List<AnswerOption> AnswerOptions { get; set; } = new();
        public Guid CourseId { get; set; }
        public bool IsReadOnly => Course?.Status != 0;

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            CourseId = courseId;
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            return Page();
        }

        // Add Reading Material
        public async Task<IActionResult> OnPostAddReadingAsync(Guid courseId, Guid lessonId, string title, string content)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            var request = new CreateReadingItemRequest
            {
                LessonId = lessonId,
                Title = title,
                Content = content,
                OrderIndex = 0
            };
            var result = await _lessonItemService.CreateReadingItemAsync(request);
            if (!result.IsSuccess)
                TempData["Error"] = result.ErrorMessage;
            else
                TempData["Success"] = "Đã thêm bài đọc thành công!";
            return RedirectToPage(new { courseId });
        }

        // Add Video Material
        public async Task<IActionResult> OnPostAddVideoAsync(Guid courseId, Guid lessonId, string title, int videoSourceType, string? videoUrl, IFormFile? videoFile)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            var request = new CreateVideoItemRequest
            {
                LessonId = lessonId,
                Title = title,
                VideoSourceType = videoSourceType,
                VideoUrl = videoUrl,
                VideoFile = videoFile,
                OrderIndex = 0
            };
            var result = await _lessonItemService.CreateVideoItemAsync(request);
            if (!result.IsSuccess)
                TempData["Error"] = result.ErrorMessage;
            else
                TempData["Success"] = "Đã thêm video thành công!";
            return RedirectToPage(new { courseId });
        }

        // Add Quiz Material
        public async Task<IActionResult> OnPostAddQuizAsync(Guid courseId, Guid lessonId, string quizTitle,
            List<string> questionContents, List<string> option1Texts, List<string> option2Texts,
            List<string> option3Texts, List<string> option4Texts, List<int> correctOptions)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            var questions = new List<CreateQuizQuestionRequest>();
            for (int i = 0; i < questionContents.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(questionContents[i])) continue;

                var options = new List<CreateQuizAnswerOptionRequest>();
                var optionTexts = new[] {
                    i < option1Texts.Count ? option1Texts[i] : "",
                    i < option2Texts.Count ? option2Texts[i] : "",
                    i < option3Texts.Count ? option3Texts[i] : "",
                    i < option4Texts.Count ? option4Texts[i] : ""
                };
                var correct = i < correctOptions.Count ? correctOptions[i] : 0;

                for (int j = 0; j < 4; j++)
                {
                    if (!string.IsNullOrWhiteSpace(optionTexts[j]))
                    {
                        options.Add(new CreateQuizAnswerOptionRequest
                        {
                            Text = optionTexts[j],
                            IsCorrect = j == correct,
                            OrderIndex = j
                        });
                    }
                }

                questions.Add(new CreateQuizQuestionRequest
                {
                    Content = questionContents[i],
                    Points = 1,
                    OrderIndex = i,
                    Options = options
                });
            }

            var request = new CreateQuizItemRequest
            {
                LessonId = lessonId,
                Title = quizTitle,
                Questions = questions,
                OrderIndex = 0
            };

            var result = await _lessonItemService.CreateQuizItemAsync(request);
            if (!result.IsSuccess)
                TempData["Error"] = result.ErrorMessage;
            else
                TempData["Success"] = "Đã thêm quiz thành công!";
            return RedirectToPage(new { courseId });
        }

        // Delete Material
        public async Task<IActionResult> OnPostDeleteMaterialAsync(Guid courseId, Guid lessonItemId)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể chỉnh sửa.";
                return RedirectToPage(new { courseId });
            }
            var result = await _lessonItemService.DeleteLessonItemAsync(lessonItemId);
            if (!result.IsSuccess)
                TempData["Error"] = result.ErrorMessage;
            else
                TempData["Success"] = "Đã xóa tài liệu";
            return RedirectToPage(new { courseId });
        }

        // Submit for Review
        public async Task<IActionResult> OnPostSubmitForReviewAsync(Guid courseId)
        {
            var loaded = await LoadCourseData(courseId);
            if (!loaded) return RedirectToPage("/Teacher/Dashboard");
            if (IsReadOnly)
            {
                TempData["Error"] = "Khóa học không ở trạng thái Draft nên không thể submit.";
                return RedirectToPage(new { courseId });
            }
            var result = await _courseService.ValidateAndSubmitForReviewAsync(courseId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToPage(new { courseId });
            }
            TempData["Success"] = "Khóa học đã được gửi để duyệt thành công!";
            return RedirectToPage("/Teacher/Dashboard");
        }

        private async Task<bool> LoadCourseData(Guid courseId)
        {
            var result = await _courseService.GetCourseForEditAsync(courseId);
            if (!result.IsSuccess || result.Result == null) return false;

            var data = (CourseEditBundleResponse)result.Result;
            Course = data.Course;
            Modules = ((IEnumerable<Module>)data.Modules).ToList();
            Lessons = ((IEnumerable<Lesson>)data.Lessons).ToList();
            LessonItems = ((IEnumerable<LessonItem>)data.LessonItems).ToList();
            LessonResources = ((IEnumerable<LessonResource>)data.LessonResources).ToList();
            GradedItems = ((IEnumerable<GradedItem>)data.GradedItems).ToList();
            Questions = ((IEnumerable<Question>)data.Questions).ToList();
            AnswerOptions = ((IEnumerable<AnswerOption>)data.AnswerOptions).ToList();
            return true;
        }
    }
}
