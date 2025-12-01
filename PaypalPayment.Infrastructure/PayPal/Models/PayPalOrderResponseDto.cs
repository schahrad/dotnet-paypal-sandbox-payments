using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalOrderResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("purchase_units")]
        public List<PayPalPurchaseUnitDto> PurchaseUnits { get; set; } = new();

        [JsonPropertyName("links")]
        public List<PayPalLinkDto> Links { get; set; } = new();

        [JsonPropertyName("create_time")]
        public DateTimeOffset CreateTime { get; set; }
    }
}








