namespace PaypalPayment.Application.Payments
{
    // Result after creating a payment at PayPal (order created)
    public class PaymentCreated
    {
        public string Id { get; init; } = default!;
        public string Status { get; init; } = default!;  
        public Uri ApprovalUrl { get; init; } = default!;    // PayPal "approve" link
    }
}
