using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class PaymentRepository : GenericRepository<OnlineLearningPlatform.DataAccess.Entities.Payment>, OnlineLearningPlatform.DataAccess.IRepositories.IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Payment>> GetExpired()
        {
            return await _context.Payments.Where(p => p.Status == 0 && p.ExpiredAt != null && p.ExpiredAt < DateTime.UtcNow).ToListAsync();
        }

        public async Task<List<Payment>> GetRecentForAdminAsync(int take)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Course)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
