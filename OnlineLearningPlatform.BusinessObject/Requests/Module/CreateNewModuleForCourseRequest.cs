namespace OnlineLearningPlatform.BusinessObject.Requests.Module
{
    public class CreateNewModuleForCourseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Index { get; set; }
        public Guid CourseId { get; set; }
    }
}
