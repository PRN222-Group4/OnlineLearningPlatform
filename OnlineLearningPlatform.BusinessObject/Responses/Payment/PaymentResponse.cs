namespace OnlineLearningPlatform.BusinessObject.Responses.Payment
{
    public class PaymentResponse
    {
        public string OrderId { get; set; }          // orderCode bên PayOS
        public string PaymentLinkId { get; set; }    // id link thanh toán
        public string Description { get; set; }      // mô tả đơn hàng
        public long Amount { get; set; }

        public string Status { get; set; }           // PAID | PENDING | CANCELLED
        public bool Success { get; set; }

        public string CheckoutUrl { get; set; }      // link redirect user
    }
}
