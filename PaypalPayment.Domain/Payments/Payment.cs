namespace PaypalPayment.Domain.Payments
{
    public class Payment
    {
        public int Id { get; set; }                  // DB primary key
        public string ProviderOrderId { get; set; } = default!;  // PayPal order id, e.g. "2KT66144D3572242S"

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";

        public PaymentStatus Status { get; set; } = PaymentStatus.Created;

        public string? Description { get; set; }

        // Optional: minimal buyer info that PayPal gives you
        public string? PayerEmail { get; set; }
        public string? PayerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CapturedAt { get; set; }
    }
}
