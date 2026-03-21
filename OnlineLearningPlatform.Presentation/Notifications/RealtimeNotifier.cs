using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.Presentation.Hubs;

namespace OnlineLearningPlatform.Presentation.Notifications
{
    public class RealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<RealtimeHub> _hubContext;

        public RealtimeNotifier(IHubContext<RealtimeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyWalletUpdated(string userId, string studentEmail = "", string courseTitle = "", decimal amount = 0)
        {
            var data = new { studentEmail, courseTitle, amount };
            await _hubContext.Clients.Group($"wallet_{userId}").SendAsync("WalletUpdated", data);
            await _hubContext.Clients.Group("admins").SendAsync("WalletUpdated", data);
            Console.WriteLine($"=== WalletUpdated sent to wallet_{userId} and admins | {studentEmail} | {courseTitle} | {amount}");
        }
    }
}