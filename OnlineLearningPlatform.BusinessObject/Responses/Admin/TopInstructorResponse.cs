namespace OnlineLearningPlatform.BusinessObject.Responses.Admin
{
    public class TopInstructorResponse
    {
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }
}