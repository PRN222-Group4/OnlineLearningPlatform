using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.BusinessObject.Requests.Course
{
    public class CreateNewCourseRequest
    {
        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5 đến 200 ký tự")]
        public string Title { get; set; } = null!;

        [StringLength(300)]
        public string? Subtitle { get; set; }

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giá phải từ 0 đến 100,000,000")]
        public decimal Price { get; set; }

        public int Level { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngôn ngữ")]
        public Guid LanguageId { get; set; }

        public string? Tags { get; set; }
    }
}