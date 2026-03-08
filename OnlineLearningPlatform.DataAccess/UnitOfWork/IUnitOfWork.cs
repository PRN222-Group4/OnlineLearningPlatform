using OnlineLearningPlatform.DataAccess.IRepositories;

namespace OnlineLearningPlatform.DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        ILessonRepository Lessons { get; }
        ICourseRepository Courses { get; }
        IEnrollmentRepository Enrollments { get; }
        ILanguageRepository Languages { get; }
        IPaymentRepository Payments { get; }
        IModuleRepository Modules { get; }
        ILessonResourceRepository LessonResources { get; }
        ILessonItemRepository LessonItems { get; }
        IGradedItemRepository GradedItems { get; }
        IGradedAttemptRepository GradedAttempts { get; }
        ISubmissionAnswerOptionRepository SubmissionAnswerOptions { get; }
        IQuestionSubmissionRepository QuestionSubmissions { get; }
        IAnswerOptionRepository AnswerOptions { get; }
        IQuestionRepository Questions { get; }
        IUserLessonProgressRepository UserLessonProgresses { get; }
        IWalletRepository Wallets { get; }
        IWalletTransactionRepository WalletTransactions { get; }
        //18

        Task SaveChangeAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}