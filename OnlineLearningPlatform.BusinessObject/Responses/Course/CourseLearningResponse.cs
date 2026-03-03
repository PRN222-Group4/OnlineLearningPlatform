using OnlineLearningPlatform.BusinessObject.Responses.Module; 
using OnlineLearningPlatform.BusinessObject.Responses.Lesson;

namespace OnlineLearningPlatform.BusinessObject.Responses.Course
{
    public class CourseLearningResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Instructor { get; set; }
        public string Description { get; set; }
        public string Level { get; set; }
        public string Duration { get; set; } // vd: "42 hours"
        public double Rating { get; set; }
        public int Students { get; set; }
        public List<string> Skills { get; set; } = new List<string>();

        public List<ModuleLearningResponse> Modules { get; set; } = new List<ModuleLearningResponse>();
    }

    public class ModuleLearningResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public List<LessonLearningResponse> Lessons { get; set; } = new List<LessonLearningResponse>();
    }

    public class LessonLearningResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; } // "video", "reading", "quiz"
        public string TypeLabel { get; set; } // "Video", "Quiz"
        public string Description { get; set; }
        public string? VideoUrl { get; set; }
        public string? Content { get; set; } 
        public string Cover { get; set; }
        public bool Completed { get; set; }
        public QuizLearningResponse? Quiz { get; set; }

        public List<ResourceLearningResponse> Resources { get; set; } = new List<ResourceLearningResponse>();
    }

    public class QuizLearningResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Kind { get; set; }
        public int PassingScore { get; set; }
        public List<QuestionLearningResponse> Questions { get; set; } = new List<QuestionLearningResponse>();
    }

    public class QuestionLearningResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
        public List<string> Options { get; set; } = new List<string>(); // Chỉ cần Text đáp án, ko cần IsCorrect (với Student)
    }

    public class ResourceLearningResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } 
        public string Url { get; set; }
    }
}