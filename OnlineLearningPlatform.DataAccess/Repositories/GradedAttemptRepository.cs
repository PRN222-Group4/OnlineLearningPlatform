using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class GradedAttemptRepository : GenericRepository<GradedAttempt>, IGradedAttemptRepository
    {
        public GradedAttemptRepository(AppDbContext context) : base(context)
        {
        }
    }
}
