using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class GradedItemRepository : GenericRepository<GradedItem>, IGradedItemRepository
    {
        public GradedItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
