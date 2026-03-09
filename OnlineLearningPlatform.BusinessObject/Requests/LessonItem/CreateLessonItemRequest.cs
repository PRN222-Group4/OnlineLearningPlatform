using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.BusinessObject.Requests.LessonItem
{
    //public class CreateReadingItemRequest
    //{
    //    [Required]
    //    public Guid LessonId { get; set; }

    //    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    //    public string Title { get; set; } = null!;

    //    [Required(ErrorMessage = "Nội dung bài đọc là bắt buộc")]
    //    public string Content { get; set; } = null!;

    //    public int OrderIndex { get; set; }
    //}

    //public class CreateVideoItemRequest
    //{
    //    [Required]
    //    public Guid LessonId { get; set; }

    //    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    //    public string Title { get; set; } = null!;

    //    /// <summary>0=None, 1=YouTube, 2=Mp4Upload</summary>
    //    [Required]
    //    public int VideoSourceType { get; set; }

    //    /// <summary>YouTube URL (when VideoSourceType=1)</summary>
    //    public string? VideoUrl { get; set; }

    //    /// <summary>Mp4 file (when VideoSourceType=2)</summary>
    //    public Microsoft.AspNetCore.Http.IFormFile? VideoFile { get; set; }

    //    public int OrderIndex { get; set; }
    //}

    public class CreateQuizItemRequest
    {
        [Required]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = "Tiêu đề quiz là bắt buộc")]
        public string Title { get; set; } = null!;

        public int OrderIndex { get; set; }

        public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
    }

    public class CreateQuizQuestionRequest
    {
        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
        public string Content { get; set; } = null!;

        public decimal Points { get; set; } = 1;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public List<CreateQuizAnswerOptionRequest> Options { get; set; } = new();
    }

    public class CreateQuizAnswerOptionRequest
    {
        [Required(ErrorMessage = "Nội dung đáp án là bắt buộc")]
        public string Text { get; set; } = null!;

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }
    }

    public class UpdateLessonItemRequest
    {
        [Required]
        public Guid LessonItemId { get; set; }

        public string? Title { get; set; }

        /// <summary>For Reading items</summary>
        public string? Content { get; set; }

        /// <summary>For Video items</summary>
        public int? VideoSourceType { get; set; }
        public string? VideoUrl { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? VideoFile { get; set; }

        /// <summary>For Quiz items</summary>
        public List<CreateQuizQuestionRequest>? Questions { get; set; }
    }
}
