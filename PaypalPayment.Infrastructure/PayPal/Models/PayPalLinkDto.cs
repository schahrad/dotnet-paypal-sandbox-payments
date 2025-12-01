using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalLinkDto
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = default!;

        [JsonPropertyName("rel")]
        public string Rel { get; set; } = default!;

        [JsonPropertyName("method")]
        public string Method { get; set; } = default!;
    }
}
