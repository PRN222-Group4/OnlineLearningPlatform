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

        public async Task NotifyWalletUpdated(string userId)
        {
            await _hubContext.Clients.Group($"wallet_{userId}").SendAsync("WalletUpdated");
            Console.WriteLine($"=== WalletUpdated sent to wallet_{userId}");
        }
    }
}