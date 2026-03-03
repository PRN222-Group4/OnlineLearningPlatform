using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.Responses;
using MailKit.Net.Smtp;
using MimeKit;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        public EmailService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        public async Task<ApiResponse> SendRejectCourseEmail(
    string receiverName,
    string receiverEmail,
    string rejectReason,
    string courseTitle)
        {
            ApiResponse response = new ApiResponse();

            try
            {
                var htmlTemplate = @"
            <h3>Course Rejected</h3>
            <p>Hello <b>{{Name}}</b>,</p>
            <p>Your course <b>{{CourseTitle}}</b> has been rejected.</p>
            <p><b>Reason:</b> {{RejectReason}}</p>
            <p>Please review and resubmit.</p>
            <br/>
            <p>Best regards,<br/>HuyShop Team</p>";

                htmlTemplate = htmlTemplate
                    .Replace("{{Name}}", receiverName)
                    .Replace("{{CourseTitle}}", courseTitle)
                    .Replace("{{RejectReason}}", rejectReason);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("HuyShop", _appSettings.SMTP.Email));
                message.To.Add(new MailboxAddress(receiverName, receiverEmail));
                message.Subject = "Your course has been rejected";

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlTemplate
                }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync(_appSettings.SMTP.Email, _appSettings.SMTP.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return response.SetOk("Reject email sent");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> SendApproveCourseEmail(string receiverName, string receiverEmail, string courseTitle)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var htmlTemplate = @"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2 style='color: #059669;'>Congratulations! Course Approved</h2>
                    <p>Hello <b>{{Name}}</b>,</p>
                    <p>We are pleased to inform you that your course <b>{{CourseTitle}}</b> has been approved and is now live on our platform.</p>
                    <p>Thank you for your contribution.</p>
                    <br/>
                    <p>Best regards,<br/><b>HuyShop Team</b></p>
                </div>";

                htmlTemplate = htmlTemplate
                    .Replace("{{Name}}", receiverName)
                    .Replace("{{CourseTitle}}", courseTitle);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("HuyShop Learning", _appSettings.SMTP.Email));
                message.To.Add(new MailboxAddress(receiverName, receiverEmail));
                message.Subject = "Your course has been approved!";

                message.Body = new BodyBuilder { HtmlBody = htmlTemplate }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(_appSettings.SMTP.Email, _appSettings.SMTP.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return response.SetOk("Approve email sent");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResponse> SendShortAnswerNotifyToInstructor(
    string instructorName,
    string instructorEmail,
    string studentName,
    string courseTitle)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                var htmlTemplate = @"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2 style='color: #2563EB;'>New Short Answer Submission</h2>
            <p>Hello <b>{{InstructorName}}</b>,</p>
            <p>Student <b>{{StudentName}}</b> has submitted a short answer for the course:</p>
            <p><b>{{CourseTitle}}</b></p>
            <p>Please log in to the system to review and grade this submission.</p>
            <br/>
            <p>Best regards,<br/><b>HuyShop Team</b></p>
        </div>";

                htmlTemplate = htmlTemplate
                    .Replace("{{InstructorName}}", instructorName)
                    .Replace("{{StudentName}}", studentName)
                    .Replace("{{CourseTitle}}", courseTitle);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("HuyShop Learning", _appSettings.SMTP.Email));
                message.To.Add(new MailboxAddress(instructorName, instructorEmail));
                message.Subject = "New short answer submitted";

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlTemplate
                }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(_appSettings.SMTP.Email, _appSettings.SMTP.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return response.SetOk("Notify instructor email sent");
            }
            catch (Exception ex)
            {
                return response.SetBadRequest(ex.Message);
            }
        }


    }
}
