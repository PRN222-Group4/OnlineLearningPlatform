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

        //public async Task<IActionResult> OnPostApproveAsync(Guid CourseId, bool IsApprove, string? RejectReason)
        //{
        //    if (CourseId == Guid.Empty)
        //    {
        //        TempData["Error"] = "Invalid course.";
        //        return RedirectToPage(new { status = Status });
        //    }

        //    var req = new ApproveCourseRequest { CourseId = CourseId, Status = IsApprove, RejectReason = RejectReason };
        //    var resp = await _service.ApproveCourseAsync(req);

        //    if (resp?.IsSuccess == true)
        //    {
        //        TempData["Success"] = IsApprove ? "Course approved successfully. An email has been sent to the instructor." : "Course rejected and email sent.";
        //        await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", new
        //        {
        //            courseId = CourseId,
        //            isApproved = IsApprove
        //        });
        //    }
        //    else
        //    {
        //        TempData["Error"] = resp?.ErrorMessage ?? "Operation failed.";
        //    }

        //    return RedirectToPage(new { status = Status });
        //}
        public async Task<IActionResult> OnPostApproveAsync(Guid CourseId, bool IsApprove, string? RejectReason)
        {
            if (CourseId == Guid.Empty)
            {
                TempData["Error"] = "Invalid course.";
                return RedirectToPage(new { status = Status });
            }

            Guid? instructorId = null;
            var allResp = await _service.GetAllCourseForAdminAsync(-1);
            if (allResp?.IsSuccess == true && allResp.Result is List<GetAllCourseForAdminResponse> allList)
            {
                instructorId = allList.FirstOrDefault(x => x.CourseId == CourseId)?.CreatedBy;
            }

            var req = new ApproveCourseRequest { CourseId = CourseId, Status = IsApprove, RejectReason = RejectReason };
            var resp = await _service.ApproveCourseAsync(req);

            if (resp?.IsSuccess == true)
            {
                TempData["Success"] = IsApprove
                    ? "Course approved successfully. An email has been sent to the instructor."
                    : "Course rejected and email sent.";

                var payload = new { courseId = CourseId, isApproved = IsApprove };

                await _hubContext.Clients.Group("admins").SendAsync("CourseStatusChanged", payload);

                if (instructorId != null)
                {
                    Console.WriteLine($"=== Sending CourseStatusChanged to wallet_{instructorId} ===");
                    await _hubContext.Clients.Group($"wallet_{instructorId}").SendAsync("CourseStatusChanged", payload);
                    Console.WriteLine("=== Sent to instructor! ===");
                }
                else
                {
                    Console.WriteLine("=== instructorId is NULL! ===");
                }

                await _hubContext.Clients.All.SendAsync("PublicCourseListChanged");
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

                await _hubContext.Clients.All.SendAsync("PublicCourseListChanged");
            }
            else
            {
                TempData["Error"] = resp?.ErrorMessage ?? "Delete failed.";
            }

            return RedirectToPage(new { status = Status });
        }
        public async Task<JsonResult> OnGetCoursesJsonAsync()
        {
            var resp = await _service.GetAllCourseForAdminAsync(Status);
            if (resp?.IsSuccess == true && resp.Result is List<GetAllCourseForAdminResponse> list)
            {
                return new JsonResult(list.Select(x => new {
                    courseId = x.CourseId,
                    title = x.Title,
                    description = x.Description,
                    price = x.Price,
                    image = x.Image,
                    status = x.Status
                }));
            }
            return new JsonResult(new List<object>());
        }
    }
}