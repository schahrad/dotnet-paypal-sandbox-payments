using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalPurchaseUnitDto
    {
        [JsonPropertyName("amount")]
        public PayPalAmountDto Amount { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("payments")]
        public PayPalPaymentsDto? Payments { get; set; }  

    }
}
