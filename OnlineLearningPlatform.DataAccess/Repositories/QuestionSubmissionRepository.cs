using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class QuestionSubmissionRepository : GenericRepository<QuestionSubmission>, IQuestionSubmissionRepository
    {
        public QuestionSubmissionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
