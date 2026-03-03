using OnlineLearningPlatform.BusinessObject.IServices;
using Microsoft.AspNetCore.Http;
using OnlineLearningPlatform.BusinessObject.DTOs;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public ClaimService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public ClaimDTO GetUserClaim()
        {
            var user = _contextAccessor.HttpContext.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            var tokenUserId = _contextAccessor.HttpContext!.User.FindFirst("UserId");
            var tokenUserRole = _contextAccessor.HttpContext!.User.FindFirst("Role");
            if (tokenUserId == null)
            {
                throw new ArgumentNullException("UserId can not be found!");
            }

            if (tokenUserRole == null)
            {
                throw new ArgumentNullException("Role claim can not be found!");
            }

            var userId = Guid.Parse(tokenUserId.Value.ToString()!);

            // Role stored as integer in claim; try parse safely
            int role;
            if (!int.TryParse(tokenUserRole.Value?.ToString(), out role))
            {
                // fallback: map common role names to numeric codes
                var roleName = tokenUserRole.Value?.ToString() ?? string.Empty;
                role = roleName switch
                {
                    "Admin" => 0,
                    "Instructor" => 1,
                    "Student" => 2,
                    _ => throw new ArgumentException("Invalid role claim value")
                };
            }

            var userClaim = new ClaimDTO
            {
                Role = role,
                UserId = userId,
            };
            return userClaim;
        }
    }
}
