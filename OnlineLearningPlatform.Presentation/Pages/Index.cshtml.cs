//Index.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.Presentation.ViewModels.Course;
using System.Text.Json;
using System.Xml.Linq;

namespace OnlineLearningPlatform.Presentation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICourseService _service;

        public IndexModel(ILogger<IndexModel> logger, ICourseService service)
        {
            _logger = logger;
            _service = service;
        }

        public List<CourseResponse> Courses { get; set; } = new();
        public async Task<IActionResult> OnGetAsync()
        {
            // If the current user is an admin, redirect to the admin layout page
            if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToPage("/Admin/Courses");
            }

            var response = await _service.GetCoursesByStatusAsync(2);
            if (!response.IsSuccess || response.Result == null)
                return Page();

            if (response.Result is List<CourseResponse> list)
            {
                Courses = list;
            }

            return Page();
        }
    }
}
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using OnlineLearningPlatform.BusinessObject.IServices;
//using OnlineLearningPlatform.BusinessObject.Responses.Course;

//namespace OnlineLearningPlatform.Presentation.Pages
//{
//    public class IndexModel : PageModel
//    {
//        private readonly ICourseService _service;
//        public IndexModel(ICourseService service) => _service = service;

//        public List<CourseResponse> Courses { get; set; } = new();

//        public async Task<IActionResult> OnGetAsync()
//        {
//            // Redirect admin to admin panel
//            if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
//                return RedirectToPage("/Admin/Overview");

//            var resp = await _service.GetCoursesByStatusAsync(2); // Status 2 = Published
//            if (resp?.IsSuccess == true && resp.Result is List<CourseResponse> list)
//                Courses = list;

//            return Page();
//        }
//    }
//}