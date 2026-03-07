using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.User;
using OnlineLearningPlatform.BusinessObject.Responses.Auth;

namespace OnlineLearningPlatform.Presentation.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnGet()
        {
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full name")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }

            public IFormFile? ImageFile { get; set; }

            [Required]
            public string Role { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // return validation errors to client
                return new BadRequestObjectResult(ModelState);
            }

            // If user selects Instructor role, disallow student/edu emails
            var emailLower = (Input.Email ?? string.Empty).ToLowerInvariant();
            bool isStudentEmail = false;
            if (emailLower.Contains("@fpt.edu") || emailLower.EndsWith(".edu") || emailLower.EndsWith(".edu.vn"))
            {
                isStudentEmail = true;
            }
            if (Input.Role?.Equals("Instructor", StringComparison.OrdinalIgnoreCase) == true && isStudentEmail)
            {
                ModelState.AddModelError("Input.Email", "Instructor accounts require a non-student email (not .edu or @fpt.edu). Please use a different email.");
                return new BadRequestObjectResult(ModelState);
            }

            // Map to business RegisterRequest
            var model = new RegisterRequest
            {
                FullName = Input.FullName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword,
                Role = Input.Role,
                ImageFile = Input.ImageFile ?? Request.Form.Files.GetFile("ImageFile")
            };

            var result = await _authService.RegisterAsync(model);
            if (result == null || !result.IsSuccess)
            {
                return new BadRequestObjectResult(result?.ErrorMessage ?? "Registration failed");
            }

            // Auto sign-in after successful registration if the service returned a User
            try
            {
                var user = result.Result as RegisterResponse;
                if (user != null)
                {
                    var roleName = user.Role switch
                    {
                        0 => "Admin",
                        1 => "Instructor",
                        2 => "Student",
                        _ => "User"
                    };

                    var claims = new List<System.Security.Claims.Claim>
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName ?? user.Email ?? ""),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleName),
                        new System.Security.Claims.Claim("Role", roleName),
                        new System.Security.Claims.Claim("Email", user.Email ?? ""),
                        new System.Security.Claims.Claim("UserId", user.UserId.ToString())
                    };
                    if (!string.IsNullOrEmpty(user.Image))
                    {
                        claims.Add(new System.Security.Claims.Claim("Avatar", user.Image));
                    }

                    var identity = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
            }
            catch
            {
                // ignore sign-in errors
            }

            return new OkResult();
        }
    }
}
