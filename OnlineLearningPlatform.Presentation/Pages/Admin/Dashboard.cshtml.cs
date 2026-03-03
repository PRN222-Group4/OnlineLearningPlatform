using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.Presentation.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public List<string> Months { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();

        public void OnGet()
        {
            Months = new List<string>
            {
                "Jan", "Feb", "Mar", "Apr", "May", "Jun"
            };

            RevenueData = new List<decimal>
            {
                1200000,
                1800000,
                1500000,
                2200000,
                2700000,
                3100000
            };
        }
    }
}
