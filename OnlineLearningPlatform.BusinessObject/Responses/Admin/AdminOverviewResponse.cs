using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using System.Collections.Generic;

namespace OnlineLearningPlatform.BusinessObject.Responses.Admin
{
    public class AdminOverviewResponse
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }

        public string? TopCourseTitle { get; set; }
        public int TopCourseEnrolls { get; set; }
        public string? TopInstructorName { get; set; }
        public int TopInstructorStudents { get; set; }

        public List<PaymentRecord>? RecentPayments { get; set; }
    }
}