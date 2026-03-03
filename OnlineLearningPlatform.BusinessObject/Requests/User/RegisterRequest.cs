using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatform.BusinessObject.Requests.User
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string Role { get; set; }
    }
}
