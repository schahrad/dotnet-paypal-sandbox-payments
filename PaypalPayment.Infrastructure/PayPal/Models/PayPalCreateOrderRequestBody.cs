using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalCreateOrderRequestBody
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = "CAPTURE";

        [JsonPropertyName("purchase_units")]
        public List<PayPalPurchaseUnitDto> PurchaseUnits { get; set; } = new();

        [JsonPropertyName("application_context")]
        public PayPalApplicationContext ApplicationContext { get; set; } = new();
    }

    public class PayPalApplicationContext
    {
        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; } = default!;

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; } = default!;
    }
}





