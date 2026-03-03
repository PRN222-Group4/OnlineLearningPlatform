using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.IRepositories
{
    public interface IPaymentRepository : IGenericRepository<OnlineLearningPlatform.DataAccess.Entities.Payment>
    {
        Task<List<Payment>> GetExpired();
    }
}
