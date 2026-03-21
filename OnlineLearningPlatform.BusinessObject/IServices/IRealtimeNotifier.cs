using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IRealtimeNotifier
    {
        Task NotifyWalletUpdated(string userId, string studentEmail = "", string courseTitle = "", decimal amount = 0);
    }
}
