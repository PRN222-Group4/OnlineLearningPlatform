using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.User;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnlineLearningPlatform.BusinessObject.Responses;
using System.Text.Json;


namespace OnlineLearningPlatform.Presentation.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult OnGet()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                // Already logged in - redirect to home
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostLogin()
        {
            LoginRequest request = null;

            // If content-type is JSON, read and deserialize
            var contentType = Request.ContentType ?? string.Empty;
            if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var sr = new StreamReader(Request.Body);
                    var body = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        request = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
                catch
                {
                    // ignore JSON parse errors and fallback to form
                }
            }

            // Fallback to form values (e.g., traditional form post or antiforgery hidden field present)
            if (request == null && Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var email = form["Email"].FirstOrDefault();
                var password = form["Password"].FirstOrDefault();
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    request = new LoginRequest { Email = email, Password = password };
                }
            }

            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Missing credentials");

            var result = await _authService.LoginAsync(new BusinessObject.Requests.User.LoginRequest { Email = request.Email, Password = request.Password });

            if (result == null || !result.IsSuccess)
            {
                var message = result?.ErrorMessage ?? "Invalid credentials";
                return BadRequest(message);
            }

            // result.Result is expected to be a JWT token string
            var token = result.Result as string;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid login response");
            }

            // Parse token to extract claims (or if AuthService provided user info, use that)
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();

            // Ensure the identity knows which claim type represents role so IsInRole works correctly
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // choose redirect based on role (admin -> admin area)
            var redirectUrl = Url.Content("~/");
            try
            {
                if (principal.IsInRole("Admin"))
                {
                    redirectUrl = Url.Content("~/Admin");
                }
            }
            catch
            {
                // ignore role check errors
            }

            // If AJAX request, return JSON instructing client to redirect
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return new JsonResult(new { redirect = redirectUrl });
            }

            // Non-AJAX - perform server redirect
            return Redirect(redirectUrl);
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
