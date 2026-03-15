using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Presentation.Hubs;


namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class PendingModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<RealtimeHub> _hubContext;

        public PendingModel(ICourseService courseService, IHubContext<RealtimeHub> hubContext)
        {
            _courseService = courseService;
            _hubContext = hubContext;
        }

        //        public List<PendingCourseReviewResponse> PendingCourses { get; set; } = new();

        //        public async Task OnGetAsync()
        //        {
        //            var result = await _courseService.GetPendingCoursesForAdminAsync();
        //            if (result.IsSuccess && result.Result != null)
        //            {
        //                PendingCourses = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new List<PendingCourseReviewResponse>();
        //            }
        //        }
        //    }
        //}
        //        public List<PendingCourseReviewResponse> PendingCourses { get; set; } = new();

        //        public async Task OnGetAsync()
        //        {
        //            var result = await _courseService.GetPendingCoursesForAdminAsync();
        //            if (result.IsSuccess && result.Result != null)
        //                PendingCourses = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new();
        //        }

        //        public async Task<JsonResult> OnGetPendingJsonAsync()
        //        {
        //            var result = await _courseService.GetPendingCoursesForAdminAsync();
        //            if (result.IsSuccess && result.Result != null)
        //            {
        //                var list = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new();
        //                return new JsonResult(list.Select(c => new
        //                {
        //                    courseId = c.CourseId,
        //                    title = c.Title,
        //                    subtitle = c.Subtitle,
        //                    image = c.Image,
        //                    price = c.Price,
        //                    level = c.Level,
        //                    submittedAt = c.SubmittedAt
        //                }));
        //            }
        //            return new JsonResult(new List<object>());
        //        }
        //    }
        //}
        public List<PendingCourseReviewResponse> PendingCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var result = await _courseService.GetPendingCoursesForAdminAsync();
            if (result.IsSuccess && result.Result != null)
                PendingCourses = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new();
        }

        public async Task<JsonResult> OnGetPendingJsonAsync()
        {
            var result = await _courseService.GetPendingCoursesForAdminAsync();
            if (result.IsSuccess && result.Result != null)
            {
                var list = (result.Result as IEnumerable<PendingCourseReviewResponse>)?.ToList() ?? new();
                return new JsonResult(list.Select(c => new
                {
                    courseId = c.CourseId,
                    title = c.Title,
                    subtitle = c.Subtitle,
                    image = c.Image,
                    price = c.Price,
                    level = c.Level,
                    submittedAt = c.SubmittedAt
                }));
            }
            return new JsonResult(new List<object>());
        }
    }
}