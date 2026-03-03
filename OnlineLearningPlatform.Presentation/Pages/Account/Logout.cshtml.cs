using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.Presentation.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IWebHostEnvironment _env;

        public LogoutModel(ILogger<LogoutModel> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Logout requested. Cookies: {Cookies}", Request.Cookies.Keys);

                // Sign out the cookie authentication
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // also delete common auth cookie name(s) to ensure browser removes cookie
                try
                {
                    Response.Cookies.Delete(".AspNetCore.Cookies");
                    Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete cookies after signout");
                }

                // If this is an AJAX request, return JSON instructing client to redirect
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { redirect = Url.Content("~/") });
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during logout");
                // In development return details to help debugging
                if (_env.IsDevelopment())
                {
                    return new ObjectResult(new { error = ex.Message, stack = ex.StackTrace }) { StatusCode = 500 };
                }
                return StatusCode(500);
            }
        }
    }
}
