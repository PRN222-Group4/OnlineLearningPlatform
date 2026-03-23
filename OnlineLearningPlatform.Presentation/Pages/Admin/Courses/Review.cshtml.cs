using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.Presentation.Hubs;

namespace OnlineLearningPlatform.Presentation.Pages.Admin.Courses
{
    public class ReviewModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public ReviewModel(ICourseService courseService, IHubContext<RealtimeHub> hubContext)
        {
            _courseService = courseService;
            _hubContext = hubContext;
        }

        public CourseEditSummaryResponse Course { get; set; } = new();
        public List<CourseModuleEditResponse> Modules { get; set; } = new();
        public List<CourseLessonEditResponse> Lessons { get; set; } = new();
        public List<CourseLessonItemEditResponse> LessonItems { get; set; } = new();
        public List<CourseLessonResourceEditResponse> LessonResources { get; set; } = new();
        public List<CourseGradedItemEditResponse> GradedItems { get; set; } = new();
        public List<CourseQuestionEditResponse> Questions { get; set; } = new();
        public List<CourseAnswerOptionEditResponse> AnswerOptions { get; set; } = new();

        [BindProperty]
        public string? RejectReason { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid courseId)
        {
            var editResult = await _courseService.GetCourseForEditAsync(courseId);
            if (!editResult.IsSuccess || editResult.Result == null)
                return RedirectToPage("/Admin/Courses/Pending");

            var data = (CourseEditBundleResponse)editResult.Result;
            Course = data.Course;
            if (Course.Status != 1) return RedirectToPage("/Admin/Courses/Pending");

            Modules = data.Modules.ToList();
            Lessons = data.Lessons.ToList();
            LessonItems = data.LessonItems.ToList();
            LessonResources = data.LessonResources.ToList();
            GradedItems = data.GradedItems.ToList();
            Questions = data.Questions.ToList();
            AnswerOptions = data.AnswerOptions.ToList();
            return Page();
        }

        // Hàm phụ trợ lấy ID ông thầy để bắn thông báo
        private async Task<Guid?> GetInstructorIdAsync(Guid courseId)
        {
            var allResp = await _courseService.GetAllCourseForAdminAsync(-1);
            if (allResp?.IsSuccess == true && allResp.Result is List<GetAllCourseForAdminResponse> allList)
            {
                return allList.FirstOrDefault(x => x.CourseId == courseId)?.CreatedBy;
            }
            return null;
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid courseId)
        {
<<<<<<< HEAD
            var instructorId = await GetInstructorIdAsync(courseId);

            var request = new ApproveCourseRequest
            {
                CourseId = courseId,
                Status = true
            };
=======
            // Lấy instructorId trước khi approve
            Guid? instructorId = null;
            var allResp = await _courseService.GetAllCourseForAdminAsync(-1);
            if (allResp?.IsSuccess == true && allResp.Result is List<GetAllCourseForAdminResponse> allList)
                instructorId = allList.FirstOrDefault(x => x.CourseId == courseId)?.CreatedBy;

            var request = new ApproveCourseRequest { CourseId = courseId, Status = true };
>>>>>>> main
            var result = await _courseService.ApproveCourseAsync(request);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Duyệt khóa học thành công";
<<<<<<< HEAD
                var payload = new { courseId = courseId, isApproved = true };

                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", payload);
                if (instructorId != null)
                {
                    await _hubContext.Clients.Group($"wallet_{instructorId}").SendAsync("CourseStatusChanged", payload);
                }
                await _hubContext.Clients.All.SendAsync("PublicCourseListChanged");
=======

                var payload = new { courseId, isApproved = true };
                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", payload);

                if (instructorId.HasValue && instructorId.Value != Guid.Empty)
                {
                    Console.WriteLine($"=== Notify instructor wallet_{instructorId} - Approved ===");
                    await _hubContext.Clients.Group($"wallet_{instructorId}").SendAsync("CourseStatusChanged", payload);
                }
>>>>>>> main
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Duyệt khóa học thất bại";
            }

            return RedirectToPage("/Admin/Courses/Pending");
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid courseId)
        {
            if (string.IsNullOrWhiteSpace(RejectReason))
            {
                TempData["Error"] = "Lý do từ chối là bắt buộc";
                return RedirectToPage(new { courseId });
            }

<<<<<<< HEAD
            var instructorId = await GetInstructorIdAsync(courseId);

            var request = new ApproveCourseRequest
            {
                CourseId = courseId,
                Status = false,
                RejectReason = RejectReason
            };
=======
            // Lấy instructorId trước khi reject
            // THAY:
            Guid? instructorId = null;
            var allResp = await _courseService.GetAllCourseForAdminAsync(-1);
            if (allResp?.IsSuccess == true && allResp.Result is List<GetAllCourseForAdminResponse> allList)
                instructorId = allList.FirstOrDefault(x => x.CourseId == courseId)?.CreatedBy;

            var request = new ApproveCourseRequest { CourseId = courseId, Status = false, RejectReason = RejectReason };
>>>>>>> main
            var result = await _courseService.ApproveCourseAsync(request);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Đã từ chối khóa học";
<<<<<<< HEAD
                var payload = new { courseId = courseId, isApproved = false };

                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", payload);
                if (instructorId != null)
                {
                    await _hubContext.Clients.Group($"wallet_{instructorId}").SendAsync("CourseStatusChanged", payload);
                }
                await _hubContext.Clients.All.SendAsync("PublicCourseListChanged");
=======

                var payload = new { courseId, isApproved = false };
                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", payload);

                if (instructorId.HasValue && instructorId.Value != Guid.Empty)
                {
                    Console.WriteLine($"=== Notify instructor wallet_{instructorId} - Rejected ===");
                    await _hubContext.Clients.Group($"wallet_{instructorId}").SendAsync("CourseStatusChanged", payload);
                }
>>>>>>> main
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Từ chối khóa học thất bại";
            }

            return RedirectToPage("/Admin/Courses/Pending");
        }
    }
}