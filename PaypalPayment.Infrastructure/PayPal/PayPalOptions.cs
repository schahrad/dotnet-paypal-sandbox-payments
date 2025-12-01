namespace PaypalPayment.Infrastructure.PayPal
{
    public class PayPalOptions
    {
        // Sandbox base URL by default
        public string BaseUrl { get; init; } = "https://api-m.sandbox.paypal.com";
        public string ClientId { get; init; } = default!;
        public string ClientSecret { get; init; } = default!;

        // URL where the buyer is redirected after approving payment
        public string ReturnUrl { get; init; } = "https://localhost:5001/paypal/return";
        public string CancelUrl { get; init; } = "https://localhost:5001/paypal/cancelUrl";
    }
}
