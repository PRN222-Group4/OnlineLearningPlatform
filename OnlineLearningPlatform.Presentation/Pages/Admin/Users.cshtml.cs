using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Requests.Admin;
using OnlineLearningPlatform.BusinessObject.Responses.Admin;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IAdminService _adminService;
        public UsersModel(IAdminService adminService) => _adminService = adminService;

        public AdminUsersResponse Data { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Role { get; set; }
        [BindProperty(SupportsGet = true)] public int PageIndex { get; set; } = 1;

        // Edit form binding
        [BindProperty] public Guid EditUserId { get; set; }
        [BindProperty] public string EditFullName { get; set; } = string.Empty;
        [BindProperty] public string? EditPhone { get; set; }
        [BindProperty] public string? EditBio { get; set; }
        [BindProperty] public string? EditTitle { get; set; }
        [BindProperty] public int EditRole { get; set; }

        public string? ToastMessage { get; set; }
        public string ToastType { get; set; } = "success";

        private const int PageSize = 10;

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        // ?? Edit: save ????????????????????????????????????????????????????????
        public async Task<IActionResult> OnPostEditAsync()
        {
            var ok = await _adminService.UpdateUserAsync(EditUserId, new AdminUpdateUserRequest
            {
                FullName = EditFullName,
                PhoneNumber = EditPhone,
                Bio = EditBio,
                Title = EditTitle,
                Role = EditRole
            });

            TempData["Toast"] = ok ? "User updated successfully." : "Failed to update user.";
            TempData["ToastType"] = ok ? "success" : "error";
            return RedirectToPage(new { Search, Role, PageIndex });
        }

        // ?? Soft delete ???????????????????????????????????????????????????????
        public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
        {
            var ok = await _adminService.SoftDeleteUserAsync(userId);
            TempData["Toast"] = ok ? "User deleted." : "Failed to delete user.";
            TempData["ToastType"] = ok ? "success" : "error";
            return RedirectToPage(new { Search, Role, PageIndex });
        }

        // ?? Toggle ban ????????????????????????????????????????????????????????
        public async Task<IActionResult> OnPostToggleBanAsync(Guid userId)
        {
            var ok = await _adminService.ToggleBanUserAsync(userId);
            TempData["Toast"] = ok ? "User status updated." : "Failed to update status.";
            TempData["ToastType"] = ok ? "success" : "error";
            return RedirectToPage(new { Search, Role, PageIndex });
        }

        private async Task LoadDataAsync()
        {
            ToastMessage = TempData["Toast"] as string;
            ToastType = TempData["ToastType"] as string ?? "success";
            try
            {
                Data = await _adminService.GetUsersAsync(PageIndex, PageSize, Search, Role);
            }
            catch { Data = new AdminUsersResponse(); }
        }
    }
}