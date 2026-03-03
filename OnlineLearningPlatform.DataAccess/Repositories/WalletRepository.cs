

using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.IRepositories;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context) : base(context)
        {
        }
    }
}
