namespace PaypalPayment.Infrastructure.PayPal.Constants
{
    public static class PayPalEndpoints
    {
        public const string Orders = "/v2/checkout/orders";

        public const string GetAccessToken = "/v1/oauth2/token";
        public static string GetOrder(string id) => $"{Orders}/{id}";

        public static string CaptureOrder(string id) => $"{Orders}/{id}/capture";

    }
}
