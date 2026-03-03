

using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.IRepositories;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class LessonItemRepository : GenericRepository<LessonItem>, ILessonItemRepository
    {
        public LessonItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}
