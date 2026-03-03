using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;

        public ProfileModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string Name { get; set; } = "User";
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = "/images/default-avatar.png";

        public async Task OnGetAsync()
        {
            try
            {
                var resp = await _authService.ProfileAsync();
                if (resp != null && resp.IsSuccess && resp.Result != null)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(resp.Result);
                        var profile = JsonSerializer.Deserialize<OnlineLearningPlatform.BusinessObject.Responses.User.ProfileResponse>(json);
                        if (profile != null)
                        {
                            Name = profile.FullName ?? Name;
                            Email = profile.Email ?? Email;
                            Avatar = profile.Image ?? Avatar;
                        }
                    }
                    catch
                    {
                        // ignore deserialization errors and keep defaults
                    }
                }
            }
            catch
            {
                // ignore and show defaults
            }
        }
    }
}
