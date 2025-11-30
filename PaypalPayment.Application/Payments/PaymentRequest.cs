namespace PaypalPayment.Application.Payments
{
    public class PaymentRequest
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "EUR";
        public string Description { get; init; } = default!;
    }
}
