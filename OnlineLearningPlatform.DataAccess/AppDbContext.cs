using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.DataAccess.Entities;

namespace OnlineLearningPlatform.DataAccess;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<GradedAttempt> GradedAttempts { get; set; }

    public virtual DbSet<GradedItem> GradedItems { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonItem> LessonItems { get; set; }

    public virtual DbSet<LessonResource> LessonResources { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionSubmission> QuestionSubmissions { get; set; }

    public virtual DbSet<SubmissionAnswerOption> SubmissionAnswerOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserLessonProgress> UserLessonProgresses { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasIndex(e => e.QuestionId, "IX_AnswerOptions_QuestionId");
            entity.HasIndex(e => new { e.QuestionId, e.OrderIndex }, "IX_AnswerOptions_QuestionId_OrderIndex");

            entity.Property(e => e.AnswerOptionId).ValueGeneratedNever();

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions).HasForeignKey(d => d.QuestionId);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.LanguageId, "IX_Courses_LanguageId");
            entity.HasIndex(e => e.CreatedBy, "IX_Courses_CreatedBy");
            entity.HasIndex(e => e.Status, "IX_Courses_Status");

            entity.Property(e => e.CourseId).ValueGeneratedNever();

            entity.HasOne(d => d.Language).WithMany(p => p.Courses).HasForeignKey(d => d.LanguageId);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Enrollments_CourseId");

            entity.HasIndex(e => e.UserId, "IX_Enrollments_UserId");

            entity.Property(e => e.EnrollmentId).ValueGeneratedNever();

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<GradedAttempt>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_GradedAttempts_UserId");

            entity.HasIndex(e => e.GradedItemId, "IX_GradedAttempts_GradedItemId");

            entity.Property(e => e.GradedAttemptId).ValueGeneratedNever();

            entity.HasOne(d => d.GradedAttemptNavigation)
                  .WithMany() 
                  .HasForeignKey(d => d.GradedItemId); 

            entity.HasOne(d => d.User)
                  .WithMany(p => p.GradedAttempts)
                  .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<GradedItem>(entity =>
        {
            entity.HasIndex(e => e.LessonItemId, "IX_GradedItems_LessonItemId").IsUnique();

            entity.Property(e => e.GradedItemId).ValueGeneratedNever();

            entity.HasOne(d => d.LessonItem).WithOne(p => p.GradedItem).HasForeignKey<GradedItem>(d => d.LessonItemId);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasIndex(e => e.ModuleId, "IX_Lessons_ModuleId");
            entity.HasIndex(e => new { e.ModuleId, e.OrderIndex }, "IX_Lessons_ModuleId_OrderIndex");

            entity.Property(e => e.LessonId).ValueGeneratedNever();

            entity.HasOne(d => d.Module).WithMany(p => p.Lessons).HasForeignKey(d => d.ModuleId);
        });

        modelBuilder.Entity<LessonItem>(entity =>
        {
            entity.HasIndex(e => e.LessonId, "IX_LessonItems_LessonId");

            entity.Property(e => e.LessonItemId).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonItems).HasForeignKey(d => d.LessonId);
        });

        modelBuilder.Entity<LessonResource>(entity =>
        {
            entity.HasIndex(e => e.LessonItemId, "IX_LessonResources_LessonItemId");

            entity.Property(e => e.LessonResourceId).ValueGeneratedNever();

            entity.HasOne(d => d.LessonItem).WithMany(p => p.LessonResources).HasForeignKey(d => d.LessonItemId);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Modules_CourseId");

            entity.Property(e => e.ModuleId).ValueGeneratedNever();

            entity.HasOne(d => d.Course).WithMany(p => p.Modules).HasForeignKey(d => d.CourseId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Payments_CourseId");

            entity.HasIndex(e => e.EnrollmentId, "IX_Payments_EnrollmentId");

            entity.HasIndex(e => e.UserId, "IX_Payments_UserId");

            entity.Property(e => e.PaymentId).ValueGeneratedNever();
            entity.Property(e => e.Type).HasDefaultValue(0);

            entity.HasOne(d => d.Course).WithMany(p => p.Payments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany(p => p.Payments).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasIndex(e => e.GradedItemId, "IX_Questions_GradedItemId");
            entity.HasIndex(e => new { e.GradedItemId, e.OrderIndex }, "IX_Questions_GradedItemId_OrderIndex");

            entity.Property(e => e.QuestionId).ValueGeneratedNever();

            entity.HasOne(d => d.GradedItem).WithMany(p => p.Questions).HasForeignKey(d => d.GradedItemId);
        });

        modelBuilder.Entity<QuestionSubmission>(entity =>
        {
            entity.HasIndex(e => e.GradedAttemptId, "IX_QuestionSubmissions_GradedAttemptId");

            entity.HasIndex(e => e.QuestionId, "IX_QuestionSubmissions_QuestionId");

            entity.Property(e => e.QuestionSubmissionId).ValueGeneratedNever();

            entity.HasOne(d => d.GradedAttempt).WithMany(p => p.QuestionSubmissions).HasForeignKey(d => d.GradedAttemptId);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionSubmissions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SubmissionAnswerOption>(entity =>
        {
            entity.HasIndex(e => e.AnswerOptionId, "IX_SubmissionAnswerOptions_AnswerOptionId");

            entity.HasIndex(e => e.QuestionSubmissionId, "IX_SubmissionAnswerOptions_QuestionSubmissionId");

            entity.Property(e => e.SubmissionAnswerOptionId).ValueGeneratedNever();

            entity.HasOne(d => d.AnswerOption).WithMany(p => p.SubmissionAnswerOptions)
                .HasForeignKey(d => d.AnswerOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.QuestionSubmission).WithMany(p => p.SubmissionAnswerOptions)
                .HasForeignKey(d => d.QuestionSubmissionId)
                .HasConstraintName("FK_SubmissionAnswerOptions_QuestionSubmissions_QuestionSubmiss~");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserLessonProgress>(entity =>
        {
            entity.HasKey(e => e.LessonProgressId);

            entity.HasIndex(e => e.LessonId, "IX_UserLessonProgresses_LessonId");

            entity.HasIndex(e => e.UserId, "IX_UserLessonProgresses_UserId");

            entity.Property(e => e.LessonProgressId).ValueGeneratedNever();

            entity.HasOne(d => d.Lesson).WithMany(p => p.UserLessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.User).WithMany(p => p.UserLessonProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Wallets_UserId").IsUnique();

            entity.Property(e => e.WalletId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Wallet).HasForeignKey<Wallet>(d => d.UserId);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasIndex(e => e.PaymentId, "IX_WalletTransactions_PaymentId");

            entity.HasIndex(e => e.WalletId, "IX_WalletTransactions_WalletId");

            entity.Property(e => e.WalletTransactionId).ValueGeneratedNever();

            entity.HasOne(d => d.Payment).WithMany(p => p.WalletTransactions).HasForeignKey(d => d.PaymentId);

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions).HasForeignKey(d => d.WalletId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
