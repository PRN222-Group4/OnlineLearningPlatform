using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Module;
using OnlineLearningPlatform.BusinessObject.Requests.Lesson;
using OnlineLearningPlatform.BusinessObject.Requests.LessonItem;
using OnlineLearningPlatform.BusinessObject.Responses.Module;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Presentation.Hubs;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Courses
{
    public class EditModulesModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly ILessonItemService _lessonItemService;
        private readonly IHubContext<RealtimeHub> _hubContext;
        private readonly IAwsAiService _awsAiService;


        public EditModulesModel(IAwsAiService awsAiService, IModuleService moduleService, ICourseService courseService, ILessonService lessonService, ILessonItemService lessonItemService, IHubContext<RealtimeHub> hubContext)
        {
            _moduleService = moduleService;
            _courseService = courseService;
            _lessonService = lessonService;
            _lessonItemService = lessonItemService;
            _hubContext = hubContext;
            _awsAiService = awsAiService;
        }

        public List<ModuleResponse> Modules { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            var response = await _moduleService.GetModulesByCourseAsync(courseId);
            if (response.IsSuccess && response.Result != null)
            {
                Modules = (List<ModuleResponse>)response.Result;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostGenerateQuizFromPdfAsync(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return new JsonResult(new { success = false, message = "Vui lòng chọn file PDF." });

            var result = await _awsAiService.GenerateQuizFromPdfAsync(pdfFile);

            if (!result.IsSuccess)
                return new JsonResult(new { success = false, message = result.ErrorMessage });

            return Content(result.Result.ToString()!, "application/json");
        }

        // 1.MODULE
        public async Task<IActionResult> OnPostCreateModuleAsync(Guid courseId, CreateNewModuleForCourseRequest Input)
        {
            Input.CourseId = courseId;
            var response = await _moduleService.CreateNewModuleForCourseAsync(Input);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = "Đã thêm Module mới!";
            return RedirectToPage(new { courseId });
        }

        // 2. LESSON
        public async Task<IActionResult> OnPostCreateLessonAsync(Guid courseId, CreateNewLessonForModuleRequest LessonInput)
        {
            if (string.IsNullOrEmpty(LessonInput.Content))
                LessonInput.Content = "Lesson content";

            var response = await _lessonService.CreateNewLessonForModuleAsync(LessonInput);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = $"Đã thêm bài học '{LessonInput.Title}' thành công!";

            return RedirectToPage(new { courseId });
        }

        // 3. TẠO VIDEO MATERIAL
        public async Task<IActionResult> OnPostCreateVideoAsync(Guid courseId, CreateVideoItemRequest VideoInput)
        {
            // Set type = 2 
            VideoInput.VideoSourceType = 2;

            var response = await _lessonItemService.CreateVideoItemAsync(VideoInput);

            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = "Video đã được upload và xử lý thành công!";

            return RedirectToPage(new { courseId });
        }

        // 4. SUBMIT DRAFT
        public async Task<IActionResult> OnPostSubmitCourseAsync(Guid courseId)
        {
            var response = await _courseService.ValidateAndSubmitForReviewAsync(courseId);
            if (!response.IsSuccess)
            {
                TempData["Error"] = response.ErrorMessage;
                return RedirectToPage(new { courseId });
            }

            await _hubContext.Clients.Group("admins").SendAsync("NewPendingCourse", new
            {
                courseId = courseId,
                title = "New course submitted"
            });

            TempData["Success"] = "Khóa học đã được nộp để Admin xét duyệt!";
            return RedirectToPage("/Teacher/Dashboard");
        }

        public async Task<IActionResult> OnPostCreateWritingAsync(Guid courseId, CreateWritingItemRequest WritingInput)
        {
            var response = await _lessonItemService.CreateWritingItemAsync(WritingInput);

            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = "IELTS Writing Task đã được tạo thành công! Sẵn sàng cho AI chấm điểm.";

            return RedirectToPage(new { courseId });
        }


        public async Task<IActionResult> OnPostCreateReadingAsync(Guid courseId, CreateReadingItemRequest ReadingInput)
        {
            var response = await _lessonItemService.CreateReadingItemAsync(ReadingInput);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = "Đã thêm bài đọc thành công!";
            return RedirectToPage(new { courseId });
        }

        // 5. SPEAKING
        public async Task<IActionResult> OnPostCreateSpeakingAsync(Guid courseId, CreateSpeakingItemRequest SpeakingInput)
        {
            var response = await _lessonItemService.CreateSpeakingItemAsync(SpeakingInput);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = "Đã tạo IELTS Speaking Task thành công!";
            return RedirectToPage(new { courseId });
        }

        // 6. QUIZ 
        public async Task<IActionResult> OnPostCreateQuizAsync(Guid courseId, CreateQuizItemRequest QuizInput)
        {
            if (string.IsNullOrEmpty(QuizInput.Title)) QuizInput.Title = "Quiz Assessment";

            var response = await _lessonItemService.CreateQuizItemAsync(QuizInput);
            if (!response.IsSuccess) TempData["Error"] = response.ErrorMessage;
            else TempData["Success"] = $"Đã tạo Quiz với {QuizInput.Questions?.Count ?? 0} câu hỏi thành công!";
            return RedirectToPage(new { courseId });
        }
    }
}