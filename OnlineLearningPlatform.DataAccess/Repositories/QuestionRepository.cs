namespace OnlineLearningPlatform.DataAccess.Repositories
{
    public class QuestionRepository : GenericRepository<OnlineLearningPlatform.DataAccess.Entities.Question>, OnlineLearningPlatform.DataAccess.IRepositories.IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
