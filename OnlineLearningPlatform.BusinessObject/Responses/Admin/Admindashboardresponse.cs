using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.BusinessObject.Responses.Admin
{
    public class AdminDashboardResponse
    {
        // Revenue theo tháng (năm nay)
        public List<string> RevenueMonths { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();

        // Revenue năm trước (cùng kỳ so sánh)
        public List<decimal> PrevRevenueData { get; set; } = new();
        public string PrevPeriodLabel { get; set; } = "";

        // Enrollments theo tháng (năm nay)
        public List<string> EnrollmentMonths { get; set; } = new();
        public List<int> EnrollmentData { get; set; } = new();

        // Enrollments năm trước (cùng kỳ)
        public List<int> PrevEnrollmentData { get; set; } = new();

        // Tỉ lệ Role users
        public int AdminCount { get; set; }
        public int InstructorCount { get; set; }
        public int StudentCount { get; set; }

        // Top 5 courses by enrollments
        public List<string> TopCourseTitles { get; set; } = new();
        public List<int> TopCourseEnrolls { get; set; } = new();

        // Top 5 courses by revenue
        public List<string> TopCourseRevenueTitle { get; set; } = new();
        public List<decimal> TopCourseRevenueData { get; set; } = new();

        // Top 5 instructors by revenue
        public List<string> TopInstructorNames { get; set; } = new();
        public List<decimal> TopInstructorRevenue { get; set; } = new();

        public decimal? RevenueGrowth { get; set; }
        public decimal? EnrollmentGrowth { get; set; }
        public List<string> TopInstructorEnrollNames { get; set; } = new();
        public List<int> TopInstructorEnrollData { get; set; } = new();
        public List<string> UserGrowthLabels { get; set; } = new();
        public List<int> UserGrowthData { get; set; } = new();
        public List<int> PrevUserGrowthData { get; set; } = new();
        public List<string> TopStudentNames { get; set; } = new();
        public List<decimal> TopStudentSpending { get; set; } = new();
        public List<string> TopCourseCreatorEmails { get; set; } = new();
        public List<string> TopCourseRevenueCreatorEmails { get; set; } = new();
        public List<string> TopInstructorEmails { get; set; } = new();
        public List<string> TopInstructorEnrollEmails { get; set; } = new();
        public List<string> TopStudentEmails { get; set; } = new();
    }
}
