using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Course;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.Presentation.Hubs;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class CoursesModel : PageModel
    {
        private readonly ICourseService _service;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public CoursesModel(ICourseService service, IHubContext<RealtimeHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [BindProperty(SupportsGet = true)]
        public int Status { get; set; } = -1;
        public List<CourseResponse> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var resp = await _service.GetAllCourseForAdminAsync(Status);
            if (resp?.IsSuccess == true && resp.Result is List<GetAllCourseForAdminResponse> list)
            {
                Courses = list.Select(x => new CourseResponse
                {
                    CourseId = x.CourseId,
                    Title = x.Title,
                    Description = x.Description,
                    Price = x.Price,
                    Image = x.Image,
                    Status = x.Status
                }).ToList();
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid CourseId, bool IsApprove, string? RejectReason)
        {
            if (CourseId == Guid.Empty)
            {
                TempData["Error"] = "Invalid course.";
                return RedirectToPage(new { status = Status });
            }

            var req = new ApproveCourseRequest { CourseId = CourseId, Status = IsApprove, RejectReason = RejectReason };
            var resp = await _service.ApproveCourseAsync(req);

            if (resp?.IsSuccess == true)
            {
                TempData["Success"] = IsApprove ? "Course approved successfully. An email has been sent to the instructor." : "Course rejected and email sent.";
                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", new
                {
                    courseId = CourseId,
                    isApproved = IsApprove
                });
            }
            else
            {
                TempData["Error"] = resp?.ErrorMessage ?? "Operation failed.";
            }

            return RedirectToPage(new { status = Status });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var resp = await _service.DeleteCourseAsync(id);
            if (resp?.IsSuccess == true)
            {
                TempData["Success"] = "Course deleted.";
                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", new
                {
                    courseId = id,
                    isApproved = false
                });
            }
            else
            {
                TempData["Error"] = resp?.ErrorMessage ?? "Delete failed.";
            }

            return RedirectToPage(new { status = Status });
        }
    }
}