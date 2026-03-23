using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.BusinessObject.IServices;
using OnlineLearningPlatform.BusinessObject.Responses.Wallet;
using OnlineLearningPlatform.BusinessObject.Responses.Payment;
using OnlineLearningPlatform.BusinessObject.Responses.Course;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineLearningPlatform.Presentation.Pages.Teacher.Wallet
{
    public class IndexModel : PageModel
    {
        private readonly IWalletService _walletService;
        private readonly IPaymentService _paymentService;
        private readonly ICourseService _courseService;

        public IndexModel(IWalletService walletService, IPaymentService paymentService, ICourseService courseService)
        {
            _walletService = walletService;
            _paymentService = paymentService;
            _courseService = courseService;
        }

        public WalletResponse MyWallet { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // Filter
        [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public string? ToDate { get; set; }

        // Chart data
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();
        public List<decimal> GrossData { get; set; } = new();
        public List<int> EnrollData { get; set; } = new();
        public List<(string Title, int Enrollments, decimal Revenue)> TopCoursesByRevenue { get; set; } = new();


        // Top courses
        public List<(string Title, int Enrollments, decimal Revenue)> TopCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Wallet
            var res = await _walletService.GetMyWalletAsync();
            if (res.IsSuccess && res.Result != null)
            {
                var json = JsonSerializer.Serialize(res.Result);
                MyWallet = JsonSerializer.Deserialize<WalletResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else { ErrorMessage = res.ErrorMessage; }

            // Parse dates
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            DateTime start = DateTime.UtcNow.AddYears(-1);
            DateTime end = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParse(FromDate, out var fd))
                start = DateTime.SpecifyKind(fd.Date, DateTimeKind.Utc);
            if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParse(ToDate, out var td))
                end = DateTime.SpecifyKind(td.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            // Get payments
            var payRes = await _paymentService.GetSuccessfulPaymentRecordsAsync();
            var allPayments = new List<PaymentRecord>();
            if (payRes.IsSuccess && payRes.Result != null)
            {
                var pJson = JsonSerializer.Serialize(payRes.Result);
                allPayments = JsonSerializer.Deserialize<List<PaymentRecord>>(pJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            // Get instructor courses
            var courseRes = await _courseService.GetCoursesByInstructorAsync();
            var myCourses = new List<CourseResponse>();
            if (courseRes.IsSuccess && courseRes.Result != null)
            {
                var cJson = JsonSerializer.Serialize(courseRes.Result);
                myCourses = JsonSerializer.Deserialize<List<CourseResponse>>(cJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var myCourseTitles = myCourses.Select(c => c.Title).ToHashSet();

            // Filter payments to this instructor + date range
            var filtered = allPayments
        .Where(p => p.PaidAt.HasValue
            && p.PaidAt.Value >= start
            && p.PaidAt.Value <= end
            && p.CourseTitle != null
            && myCourseTitles.Contains(p.CourseTitle))
        .ToList();
            Console.WriteLine($"=== WALLET DEBUG: allPayments={allPayments.Count}, myCourses={myCourses.Count}");
            Console.WriteLine($"=== WALLET DEBUG: start={start}, end={end}");
            Console.WriteLine($"=== WALLET DEBUG: myCourseTitles={string.Join(", ", myCourseTitles)}");
            Console.WriteLine($"=== WALLET DEBUG: filtered={filtered.Count}");
            // Build monthly buckets
            var totalDays = (end - start).TotalDays;
            if (totalDays <= 31)
            {
                // Daily
                var days = Enumerable.Range(0, (int)Math.Ceiling(totalDays) + 1)
                    .Select(i => start.AddDays(i).Date).ToList();
                ChartLabels = days.Select(d => d.ToString("dd/MM")).ToList();
                RevenueData = days.Select(d => filtered.Where(p => p.PaidAt!.Value.Date == d).Sum(p => p.Amount * 0.7m)).ToList();
                GrossData = days.Select(d => filtered.Where(p => p.PaidAt!.Value.Date == d).Sum(p => p.Amount)).ToList();
                EnrollData = days.Select(d => filtered.Count(p => p.PaidAt!.Value.Date == d)).ToList();
            }
            else
            {
                // Monthly
                var months = new List<(int year, int month)>();
                var cur = new DateTime(start.Year, start.Month, 1);
                while (cur <= end) { months.Add((cur.Year, cur.Month)); cur = cur.AddMonths(1); }
                var mNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                ChartLabels = months.Select(m => $"{mNames[m.month]} {m.year}").ToList();
                RevenueData = months.Select(m => filtered.Where(p => p.PaidAt!.Value.Month == m.month && p.PaidAt.Value.Year == m.year).Sum(p => p.Amount * 0.7m)).ToList();
                GrossData = months.Select(m => filtered.Where(p => p.PaidAt!.Value.Month == m.month && p.PaidAt.Value.Year == m.year).Sum(p => p.Amount)).ToList();
                EnrollData = months.Select(m => filtered.Count(p => p.PaidAt!.Value.Month == m.month && p.PaidAt.Value.Year == m.year)).ToList();
            }

            // Top courses
            TopCourses = myCourses
            .Select(c => (
                Title: c.Title.Length > 25 ? c.Title[..25] + "..." : c.Title,
                FullTitle: c.Title,
                Enrollments: filtered.Count(p => p.CourseTitle == c.Title),
                Revenue: filtered.Where(p => p.CourseTitle == c.Title).Sum(p => p.Amount * 0.7m)
            ))
            .Where(x => x.Enrollments > 0)
            .GroupBy(x => x.FullTitle)        // group theo full title trước
            .Select(g => g.First())           // lấy 1 cái mỗi course
            .OrderByDescending(x => x.Enrollments)
            .Take(3)
            .Select(x => (x.Title, x.Enrollments, x.Revenue))
            .ToList();

                TopCoursesByRevenue = myCourses
                .Select(c => (
                    Title: c.Title.Length > 25 ? c.Title[..25] + "..." : c.Title,
                    FullTitle: c.Title,
                    Enrollments: filtered.Count(p => p.CourseTitle == c.Title),
                    Revenue: filtered.Where(p => p.CourseTitle == c.Title).Sum(p => p.Amount * 0.7m)
                ))
                .Where(x => x.Revenue > 0)
                .GroupBy(x => x.FullTitle)
                .Select(g => g.First())
                .OrderByDescending(x => x.Revenue)
                .Take(3)
                .Select(x => (x.Title, x.Enrollments, x.Revenue))
                .ToList();
        }
    }
}