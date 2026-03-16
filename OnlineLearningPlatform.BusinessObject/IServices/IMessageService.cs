using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IMessageService
    {
        Task<ApiResponse> SendMessageAsync(Guid senderId, Guid receiverId, string content);

        Task<ApiResponse> GetConversationAsync(Guid currentUserId, Guid partnerId);

        Task<ApiResponse> MarkMessagesAsReadAsync(Guid currentUserId, Guid senderId);

        Task<ApiResponse> GetChatContactsAsync(Guid currentUserId);
    }
}