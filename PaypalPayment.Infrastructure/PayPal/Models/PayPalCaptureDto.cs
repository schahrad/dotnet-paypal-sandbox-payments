namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalCaptureDto
    {
        public string Id { get; set; } = default!;
        public string Status { get; set; } = default!;
        public PayPalAmountDto Amount { get; set; } = default!;
    }
}
