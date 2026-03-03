using Microsoft.AspNetCore.Mvc;

namespace OnlineLearningPlatform.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class UiController : Controller
    {
        [HttpGet("header")]
        public IActionResult Header()
        {
            // Return the Razor Pages partial located at Pages/Shared/_Header.cshtml
            return PartialView("~/Pages/Shared/_Header.cshtml");
        }
    }
}
