using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class SubmissionAnswerOptionRepository : GenericRepository<SubmissionAnswerOption>, ISubmissionAnswerOptionRepository
    {
        public SubmissionAnswerOptionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
