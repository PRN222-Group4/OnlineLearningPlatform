namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class CourseFilterRequest
    {
        public string? SearchTerm { get; set; }
        public List<string>? Categories { get; set; } = new();
        public List<int>? Levels { get; set; } = new();
        public bool? IsFree { get; set; }
        public string SortBy { get; set; } = "popular";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }
}