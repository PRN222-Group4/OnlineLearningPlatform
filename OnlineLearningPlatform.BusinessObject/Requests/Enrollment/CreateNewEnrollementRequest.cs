namespace OnlineLearningPlatform.BusinessObject.Requests.Enrollment
{
    public class CreateNewEnrollementRequest
    {
        public Guid CourseId { get; set; }
        public int Status { get; set; }
    }
}
