namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    // DTO Root
    public class CourseStudentLearningResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Instructor { get; set; } = "Unknown Instructor";
        public string Description { get; set; } = string.Empty;
        public string Level { get; set; } = "All Levels";
        public string Duration { get; set; } = "10h 30m"; // Hardcode demo
        public double Rating { get; set; } = 5.0;
        public int Students { get; set; } = 1;
        public List<string> Skills { get; set; } = new List<string> { "Coding", "Architecture" };
        public List<ModuleStudentResponse> Modules { get; set; } = new();
    }

    public class ModuleStudentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Duration { get; set; } = "45m";
        public bool IsFinal { get; set; }
        public List<LessonStudentResponse> Lessons { get; set; } = new();
    }

    public class LessonStudentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Duration { get; set; } = "10m";
        public string Type { get; set; } = "video"; // "video", "reading", "quiz"
        public string TypeLabel { get; set; } = "Video";
        public string Description { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public string? Content { get; set; }
        public string Cover { get; set; } = "https://placehold.co/800x450";
        public bool Completed { get; set; }
        public bool Locked { get; set; }

        // Data Quiz (nếu có)
        public QuizStudentResponse? Quiz { get; set; }
        // Data Resources
        public List<ResourceStudentResponse> Resources { get; set; } = new();
        // Fake Comments
        public List<object> Comments { get; set; } = new();
    }

    public class QuizStudentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "Quiz";
        public string Subtitle { get; set; } = "Check your knowledge";
        public string Kind { get; set; } = "multiple-choice";
        public int TimeLimit { get; set; } = 15;
        public int PassingScore { get; set; } = 80;
        public List<QuestionStudentResponse> Questions { get; set; } = new();
    }

    public class QuestionStudentResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Points { get; set; } = 10;
        // Quan trọng: Dùng object để có ID
        public List<AnswerOptionDTO> Options { get; set; } = new();
        // Field cho Coding/Essay question
        public string? Language { get; set; } = "javascript";
        public string? Starter { get; set; } = "// Write code here";
    }

    public class AnswerOptionDTO
    {
        public Guid Id { get; set; } // Để submit
        public string Text { get; set; } = string.Empty;
    }

    public class ResourceStudentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "Resource";
        public string Type { get; set; } = "download";
        public string Url { get; set; } = "#";
        public string Note { get; set; } = "PDF File";
        public string Duration { get; set; } = "2MB";
    }
}