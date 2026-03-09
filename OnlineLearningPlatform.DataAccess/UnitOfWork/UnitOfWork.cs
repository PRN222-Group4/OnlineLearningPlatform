using Microsoft.EntityFrameworkCore.Storage;
using OnlineLearningPlatform.DataAccess.Entities;
using OnlineLearningPlatform.DataAccess.IRepositories;
using OnlineLearningPlatform.DataAccess.Repositories;

namespace OnlineLearningPlatform.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        public IAnswerOptionRepository AnswerOptions { get; }
        public ICourseRepository Courses { get; }
        public IEnrollmentRepository Enrollments { get; }
        public IGradedAttemptRepository GradedAttempts { get; }
        public IGradedItemRepository GradedItems { get; }
        public ILessonRepository Lessons { get; }
        public ILessonItemRepository LessonItems { get; }
        public ILanguageRepository Languages { get; }
        public ILessonResourceRepository LessonResources { get; }
        public IModuleRepository Modules { get; }
        public IPaymentRepository Payments { get; }
        public IQuestionRepository Questions { get; }
        public IQuestionSubmissionRepository QuestionSubmissions { get; }
        public ISubmissionAnswerOptionRepository SubmissionAnswerOptions { get; }
        public IUserRepository Users { get; }
        public IUserLessonProgressRepository UserLessonProgresses { get; }
        public IWalletRepository Wallets { get; }

        private IGenericRepository<Certificate>? _certificates;
        public IGenericRepository<Certificate> Certificates => _certificates ??= new GenericRepository<Certificate>(_context);
        public IWalletTransactionRepository WalletTransactions { get; }
        //18
        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Users = new UserRepository(context);
            Courses = new CourseRepository(context);
            Enrollments = new EnrollmentRepository(context);
            Lessons = new LessonRepository(context);
            Payments = new PaymentRepository(context);
            GradedItems = new GradedItemRepository(context);
            GradedAttempts = new GradedAttemptRepository(context);
            QuestionSubmissions = new QuestionSubmissionRepository(context);
            SubmissionAnswerOptions = new SubmissionAnswerOptionRepository(context);
            Modules = new ModuleRepository(context);
            LessonResources = new LessonResourceRepository(context);
            UserLessonProgresses = new UserLessonProgressRepository(context);
            Questions = new QuestionRepository(context);
            AnswerOptions = new AnswerOptionRepository(context);
            LessonItems = new LessonItemRepository(context);
            Wallets = new WalletRepository(context);
            WalletTransactions = new WalletTransactionRepository(context);
            Languages = new LanguageRepository(context);
            //19
        }

        public async Task SaveChangeAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving changes", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction!.CommitAsync();
            }
            finally
            {
                await _transaction!.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}