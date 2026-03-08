namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class CourseEditBundleResponse
    {
        public CourseEditSummaryResponse Course { get; set; } = new();
        public List<CourseModuleEditResponse> Modules { get; set; } = new();
        public List<CourseLessonEditResponse> Lessons { get; set; } = new();
        public List<CourseLessonItemEditResponse> LessonItems { get; set; } = new();
        public List<CourseLessonResourceEditResponse> LessonResources { get; set; } = new();
        public List<CourseGradedItemEditResponse> GradedItems { get; set; } = new();
        public List<CourseQuestionEditResponse> Questions { get; set; } = new();
        public List<CourseAnswerOptionEditResponse> AnswerOptions { get; set; } = new();
    }

    public class CourseEditSummaryResponse
    {
        public Guid CourseId { get; set; }
        public Guid LanguageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public int Level { get; set; }
        public string? Tags { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class CourseModuleEditResponse
    {
        public Guid ModuleId { get; set; }
        public Guid CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Index { get; set; }
    }

    public class CourseLessonEditResponse
    {
        public Guid LessonId { get; set; }
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedMinutes { get; set; }
        public int OrderIndex { get; set; }
    }

    public class CourseLessonItemEditResponse
    {
        public Guid LessonItemId { get; set; }
        public Guid LessonId { get; set; }
        public int Type { get; set; }
        public int OrderIndex { get; set; }
    }

    public class CourseLessonResourceEditResponse
    {
        public Guid LessonResourceId { get; set; }
        public Guid LessonItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ResourceType { get; set; }
        public string? ResourceUrl { get; set; }
        public string? TextContent { get; set; }
        public int VideoSourceType { get; set; }
        public int OrderIndex { get; set; }
    }

    public class CourseGradedItemEditResponse
    {
        public Guid GradedItemId { get; set; }
        public Guid LessonItemId { get; set; }
        public string? SubmissionGuidelines { get; set; }
    }

    public class CourseQuestionEditResponse
    {
        public Guid QuestionId { get; set; }
        public Guid GradedItemId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

        public decimal Points { get; set; }
    }

    public class CourseAnswerOptionEditResponse
    {
        public Guid AnswerOptionId { get; set; }
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }

    public class PendingCourseReviewResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Image { get; set; }
        public int Level { get; set; }
        public decimal Price { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
