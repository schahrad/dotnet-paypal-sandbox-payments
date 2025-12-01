using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalAmountDto
    {
        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; } = default!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = default!;
    }
}
