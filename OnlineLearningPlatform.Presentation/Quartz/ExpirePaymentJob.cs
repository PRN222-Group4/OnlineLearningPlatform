using OnlineLearningPlatform.BusinessObject.IServices;
using Quartz;

namespace OnlineLearningPlatform.Presentation.Quartz
{
    public class ExpirePaymentJob : IJob
    {
        private readonly IPaymentService _service;
        private readonly ILogger<ExpirePaymentJob> _logger;

        public ExpirePaymentJob(IPaymentService service, ILogger<ExpirePaymentJob> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogWarning("🔥 ExpirePaymentJob RUNNING at {time}", DateTime.UtcNow);
            await _service.ExpirePendingPaymentAsync();
        }
    }
}
