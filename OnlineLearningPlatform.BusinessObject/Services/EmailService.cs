using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;
using OnlineLearningPlatform.BusinessObject;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses;

namespace OnlineLearningPlatform.BusinessObject.Services
{
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(AppSettings appSettings, ILogger<EmailService> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<ApiResponse> SendRejectCourseEmail(string receiverName, string receiverEmail, string rejectReason, string courseTitle)
        {
            var response = new ApiResponse();

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

                var message = BuildMessage("HuyShop", receiverName, receiverEmail, "Your course has been rejected", htmlTemplate);
                var sent = await TrySendEmailAsync(message);
                if (!sent)
                {
                    _logger.LogWarning("Reject email fallback used for {Email}, course {CourseTitle}", receiverEmail, courseTitle);
                }

                return response.SetOk("Reject email processed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reject email failed, fallback to no-op");
                return response.SetOk("Reject email skipped (SMTP unavailable)");
            }
        }

        public async Task<ApiResponse> SendApproveCourseEmail(string receiverName, string receiverEmail, string courseTitle)
        {
            var response = new ApiResponse();
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

                var message = BuildMessage("HuyShop Learning", receiverName, receiverEmail, "Your course has been approved!", htmlTemplate);
                var sent = await TrySendEmailAsync(message);
                if (!sent)
                {
                    _logger.LogWarning("Approve email fallback used for {Email}, course {CourseTitle}", receiverEmail, courseTitle);
                }

                return response.SetOk("Approve email processed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Approve email failed, fallback to no-op");
                return response.SetOk("Approve email skipped (SMTP unavailable)");
            }
        }

        public async Task<ApiResponse> SendShortAnswerNotifyToInstructor(string instructorName, string instructorEmail, string studentName, string courseTitle)
        {
            var response = new ApiResponse();
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

                var message = BuildMessage("HuyShop Learning", instructorName, instructorEmail, "New short answer submitted", htmlTemplate);
                var sent = await TrySendEmailAsync(message);
                if (!sent)
                {
                    _logger.LogWarning("Short-answer notify email fallback used for {Email}, course {CourseTitle}", instructorEmail, courseTitle);
                }

                return response.SetOk("Notify instructor email processed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Notify email failed, fallback to no-op");
                return response.SetOk("Notify email skipped (SMTP unavailable)");
            }
        }

        private MimeMessage BuildMessage(string fromDisplayName, string receiverName, string receiverEmail, string subject, string htmlBody)
        {
            var fromEmail = string.IsNullOrWhiteSpace(_appSettings.SMTP.Email) ? "noreply@localhost" : _appSettings.SMTP.Email;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromDisplayName, fromEmail));
            message.To.Add(new MailboxAddress(receiverName, receiverEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();
            return message;
        }

        private async Task<bool> TrySendEmailAsync(MimeMessage message)
        {
            if (string.IsNullOrWhiteSpace(_appSettings.SMTP.Email) || string.IsNullOrWhiteSpace(_appSettings.SMTP.Password))
            {
                _logger.LogWarning("SMTP not configured. Skip email subject: {Subject}", message.Subject);
                return false;
            }

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(_appSettings.SMTP.Email, _appSettings.SMTP.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP send failed. Email subject: {Subject}", message.Subject);
                return false;
            }
        }
    }
}
