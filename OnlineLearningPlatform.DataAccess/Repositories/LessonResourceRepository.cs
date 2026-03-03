using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class LessonResourceRepository : GenericRepository<LessonResource>, ILessonResourceRepository
    {
        public LessonResourceRepository(AppDbContext context) : base(context)
        {
        }
    }
}
