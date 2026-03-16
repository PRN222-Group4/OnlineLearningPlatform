using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.BusinessObject.IServices;

namespace OnlineLearningPlatform.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IClaimService _claimService;
        private readonly IStorageService _storageService;
        public MessagesController(IMessageService messageService, IClaimService claimService, IStorageService storageService)
        {
            _messageService = messageService;
            _claimService = claimService;
            _storageService = storageService;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var userId = _claimService.GetUserClaim().UserId;
            var res = await _messageService.GetChatContactsAsync(userId);
            return Ok(res);
        }

        [HttpGet("conversation/{partnerId}")]
        public async Task<IActionResult> GetConversation(Guid partnerId)
        {
            var userId = _claimService.GetUserClaim().UserId;
            var res = await _messageService.GetConversationAsync(userId, partnerId);

            await _messageService.MarkMessagesAsReadAsync(userId, partnerId);

            return Ok(res);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var userName = User.Identity?.Name ?? "ChatUser";
                var url = await _storageService.UploadUserImageAsync(userName, file);
                return Ok(new { isSuccess = true, result = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, errorMessage = ex.Message });
            }
        }
    }
}