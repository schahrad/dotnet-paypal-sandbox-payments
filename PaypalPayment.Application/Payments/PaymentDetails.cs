namespace PaypalPayment.Application.Payments
{
    // Result when querying payment status
    public class PaymentDetails
    {
        public string Id { get; init; } = default!;
        public string Status { get; init; } = default!;      // e.g. CREATED, APPROVED, COMPLETED
        public decimal Amount { get; init; }
        public string Currency { get; init; } = default!;
        public DateTimeOffset CreatedAt { get; init; }
    }
}
