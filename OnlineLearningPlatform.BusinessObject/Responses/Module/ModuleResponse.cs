using OnlineLearningPlatform.BusinessObject.Responses.Lesson;

namespace OnlineLearningPlatform.BusinessObject.Responses.Module
{
    public class ModuleResponse
    {
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Index { get; set; }
        public Guid CourseId { get; set; }
        public List<LessonResponse> Lessons { get; set; } = new List<LessonResponse>();
    }
}
