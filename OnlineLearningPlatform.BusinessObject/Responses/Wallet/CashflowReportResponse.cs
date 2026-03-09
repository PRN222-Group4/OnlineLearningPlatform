namespace OnlineLearningPlatform.BusinessObject.Responses.Wallet
{
    public class CashflowReportResponse
    {
        public decimal TotalGrossRevenue { get; set; } // Tổng thu
        public decimal PlatformNetRevenue { get; set; } // 30% Đớp được
        public decimal InstructorTotalEarnings { get; set; } // 70% Của giáo viên
        public decimal TotalPaidOut { get; set; } // Tổng tiền đã chuyển khoản cho GV
        public decimal TotalPendingPayouts { get; set; } // Đang nợ chưa chuyển (Pending)
        public decimal TotalAvailableInWallets { get; set; } // Tiền GV chưa thèm rút (Balance)
        public decimal PlatformCashOnHand { get; set; } // TIỀN MẶT CÒN TRONG NGÂN HÀNG CỦA SẾP
    }
}