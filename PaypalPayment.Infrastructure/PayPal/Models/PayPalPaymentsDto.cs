using System.Text.Json.Serialization;

namespace PaypalPayment.Infrastructure.PayPal.Models
{
    public class PayPalPaymentsDto
    {
        [JsonPropertyName("captures")]
        public List<PayPalCaptureDto>? Captures { get; set; }  
    }
}
