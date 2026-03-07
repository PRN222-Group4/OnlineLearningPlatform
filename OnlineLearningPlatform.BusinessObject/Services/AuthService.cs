//AuthService.cs
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.DataAccess.UnitOfWork;
using OnlineLearningPlatform.DataAccess.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Requests.User;
using OnlineLearningPlatform.BusinessObject.DTOs;
using AutoMapper;
using OnlineLearningPlatform.BusinessObject.Responses.Auth;
using OnlineLearningPlatform.BusinessObject.Responses.User;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly IClaimService _service;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork unitOfWork, AppSettings appSettings, IFirebaseStorageService firebaseStorageService, IClaimService service, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _appSettings = appSettings;
            _firebaseStorageService = firebaseStorageService;
            _service = service;
            _mapper = mapper;
        }

        public async Task<ApiResponse> LoginAsync(LoginRequest request)
        {
            ApiResponse response = new ApiResponse();
            var account = await _unitOfWork.Users.GetAsync(u => u.Email == request.Email);
            if (account == null || !VerifyPasswordHash(request.Password, account.PasswordHash, account.PasswordSalt))
            {
                response.SetBadRequest(message: "Email or password is wrong");
                return response;
            }

            if (account.IsVerfied == false)
            {
                response.SetBadRequest(message: "Your account has been permanently banned!");
                return response;
            }
            response.SetOk(CreateToken(account));
            return response;
        }
        public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {

                var checkPassword = CheckUserPassword(request);
                if (!checkPassword)
                {
                    response.SetBadRequest(message: "Confirm password is wrong !");
                    return response;
                }
                var existingUser = await _unitOfWork.Users.GetAsync(x => x.Email == request.Email);
                if (existingUser != null)
                {
                    response.SetBadRequest(message: "The email address is already register");
                    return response;
                }

                var pass = CreatePasswordHash(request.Password);
                if (request.Role == "Admin") { return response.SetBadRequest("Role must be specified"); }
                else if (request.Role == "Instructor")
                {
                    User user = new User()
                    {
                        UserId = Guid.NewGuid(),
                        PasswordHash = pass.PasswordHash,
                        PasswordSalt = pass.PasswordSalt,
                        Email = request.Email,
                        FullName = request.FullName,
                        IsVerfied = true,
                        PhoneNumber = request.PhoneNumber,
                        Role = 1
                    };
                    if (request.ImageFile != null)
                    {
                        user.Image = await _firebaseStorageService.UploadUserImage(user.UserId.ToString(), request.ImageFile);
                    }
                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangeAsync();

                    return response.SetOk(new RegisterResponse
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        Image = user.Image
                    });
                }
                else
                {
                    User user = new User()
                    {
                        UserId = Guid.NewGuid(),
                        PasswordHash = pass.PasswordHash,
                        PasswordSalt = pass.PasswordSalt,
                        Email = request.Email,
                        FullName = request.FullName,
                        IsVerfied = true,
                        PhoneNumber = request.PhoneNumber,
                        Role = 2
                    };
                    if (request.ImageFile != null)
                    {
                        user.Image = await _firebaseStorageService.UploadUserImage(user.UserId.ToString(), request.ImageFile);
                    }
                    await _unitOfWork.Users.AddAsync(user);
                    await _unitOfWork.SaveChangeAsync();

                    return response.SetOk(new RegisterResponse
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        Image = user.Image
                    });
                }
            }
            catch (Exception ex)
            {
                return response.SetBadRequest($"Error: {ex.Message}. Details: {ex.InnerException?.Message}");
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(User user)
        {
            // Map numeric role to role name for compatibility with IsInRole
            var roleName = user.Role switch
            {
                0 => "Admin",
                1 => "Instructor",
                2 => "Student",
                _ => "User"
            };

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("Email" , user.Email!),
                new Claim("Role", roleName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Avatar", user.Image ?? string.Empty),
            };


            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                 _appSettings!.SecretToken));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        private PasswordDTO CreatePasswordHash(string password)
        {
            PasswordDTO pass = new PasswordDTO();
            using (var hmac = new HMACSHA512())
            {
                pass.PasswordSalt = hmac.Key;
                pass.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            return pass;
        }
        public bool CheckUserPassword(RegisterRequest request)
        {
            if (request.Password is null) return (false);
            return (request.Password.Equals(request.ConfirmPassword));
        }

        public async Task<ApiResponse> ProfileAsync()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var claim = _service.GetUserClaim();
                var user = await _unitOfWork.Users.GetAsync(u => u.UserId == claim.UserId);
                if (user == null)
                {
                    return response.SetNotFound("User not found");
                }
                var result = _mapper.Map<ProfileResponse>(user);
                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(message: ex.Message);
            }
        }
    }
}
