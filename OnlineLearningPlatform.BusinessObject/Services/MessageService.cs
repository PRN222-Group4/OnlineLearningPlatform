using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;
using OnlineLearningPlatform.BusinessObject.Responses.Message;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.UnitOfWork;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _uow;

        public MessageService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse> SendMessageAsync(Guid senderId, Guid receiverId, string content)
        {
            var response = new ApiResponse();
            try
            {
                var message = new Message
                {
                    MessageId = Guid.NewGuid(),
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _uow.BeginTransactionAsync();
                await _uow.Messages.AddAsync(message);
                await _uow.CommitAsync();

                var sender = await _uow.Users.GetAsync(u => u.UserId == senderId);

                var result = new MessageResponse
                {
                    MessageId = message.MessageId,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead,
                    SenderName = sender?.FullName ?? "Unknown"
                };

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetConversationAsync(Guid currentUserId, Guid partnerId)
        {
            var response = new ApiResponse();
            try
            {
                var messages = await _uow.Messages.GetQueryable()
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == partnerId) ||
                                (m.SenderId == partnerId && m.ReceiverId == currentUserId))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                var userIds = new[] { currentUserId, partnerId };
                var users = await _uow.Users.GetAllAsync(u => userIds.Contains(u.UserId));
                var userDict = users.ToDictionary(u => u.UserId, u => u.FullName ?? "Unknown");

                var result = messages.Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead,
                    SenderName = userDict.ContainsKey(m.SenderId) ? userDict[m.SenderId] : "Unknown"
                }).ToList();

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> MarkMessagesAsReadAsync(Guid currentUserId, Guid senderId)
        {
            var response = new ApiResponse();
            try
            {
                var unreadMessages = await _uow.Messages.GetQueryable()
                    .Where(m => m.SenderId == senderId && m.ReceiverId == currentUserId && !m.IsRead)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.IsRead = true;
                        _uow.Messages.Update(msg);
                    }
                    await _uow.SaveChangeAsync();
                }

                return response.SetOk(true);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> GetChatContactsAsync(Guid currentUserId)
        {
            var response = new ApiResponse();
            try
            {
                var currentUser = await _uow.Users.GetAsync(u => u.UserId == currentUserId);
                if (currentUser == null) return response.SetBadRequest("User not found");

                var messagePartners = await _uow.Messages.GetQueryable()
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                    .Distinct()
                    .ToListAsync();

                var allContactIds = messagePartners.ToList();

                if (currentUser.Role == 0)
                {
                    var teachers = await _uow.Users.GetAllAsync(u => u.Role == 1);
                    allContactIds.AddRange(teachers.Select(t => t.UserId));
                }
                else if (currentUser.Role == 1 || currentUser.Role == 2) // Nếu là Teacher hoặc Student
                {
                    var admins = await _uow.Users.GetAllAsync(u => u.Role == 0);
                    allContactIds.AddRange(admins.Select(a => a.UserId));
                }

                allContactIds = allContactIds.Distinct().ToList();
                allContactIds.Remove(currentUserId);

                var users = await _uow.Users.GetAllAsync(u => allContactIds.Contains(u.UserId));

                var result = users.Select(u => new
                {
                    id = u.UserId,
                    name = u.FullName ?? u.Email,
                    lastMessage = "Click to view chat...",
                    lastTime = "",
                    unread = 0,
                    isOnline = false
                }).ToList();

                return response.SetOk(result);
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }
    }
}