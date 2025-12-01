using PaypalPayment.Application.Common;

namespace PaypalPayment.Application.Payments
{
    // Domain request from API client to backend
    public class PaymentRequest
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; } = CurrencyCodes.Euro;
        public string Description { get; init; } = default!;
    }
}
