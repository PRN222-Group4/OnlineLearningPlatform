using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.IServices
{
    public interface IEmailService
    {
        Task<ApiResponse> SendRejectCourseEmail(string receiverName, string receiverEmail, string rejectReason, string courseTitle);

        Task<ApiResponse> SendApproveCourseEmail(string receiverName, string receiverEmail, string courseTitle);
        Task<ApiResponse> SendShortAnswerNotifyToInstructor(
   string instructorName,
   string instructorEmail,
   string studentName,
   string courseTitle);
    }
}
