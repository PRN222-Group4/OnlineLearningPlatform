using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.BusinessObject.Responses.Admin
{
    public class AdminDashboardResponse
    {
        // Revenue theo tháng
        public List<string> RevenueMonths { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();

        // Enrollments theo tháng
        public List<string> EnrollmentMonths { get; set; } = new();
        public List<int> EnrollmentData { get; set; } = new();

        // Tỉ lệ Role users
        public int AdminCount { get; set; }
        public int InstructorCount { get; set; }
        public int StudentCount { get; set; }

        // Top 5 courses by enrollments
        public List<string> TopCourseTitles { get; set; } = new();
        public List<int> TopCourseEnrolls { get; set; } = new();

        public decimal? RevenueGrowth { get; set; }
        public decimal? EnrollmentGrowth { get; set; }
    }
}
