using Microsoft.Extensions.Options;
using PaypalPayment.Application.Payments;
using PaypalPayment.Infrastructure.PayPal.Constants;
using PaypalPayment.Infrastructure.PayPal.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PaypalPayment.Infrastructure.PayPal
{
    public class PayPalPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PayPalOptions _payPalOptions;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public PayPalPaymentGateway(HttpClient httpClient, IOptions<PayPalOptions> options)
        {
            _httpClient = httpClient;
            _payPalOptions = options.Value;
        }
        public async Task<PaymentCreated> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            // 1) Get access token
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            var requestBody = new PayPalCreateOrderRequestBody()
            {
                Intent = PayPalConstants.Capture,
                PurchaseUnits = new List<PayPalPurchaseUnitDto>()
                {
                    new ()
                    {
                         Amount = new PayPalAmountDto()
                         {
                              Value = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                               CurrencyCode = request.Currency.ToUpperInvariant(),
                         },
                         Description = request.Description,
                    }
                },
                ApplicationContext = new PayPalApplicationContext()
                {
                    CancelUrl = _payPalOptions.CancelUrl,
                    ReturnUrl = _payPalOptions.ReturnUrl,
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            using var httpRequest = new HttpRequestMessage(
          HttpMethod.Post, PayPalEndpoints.Orders)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, HttpContentTypes.Json)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);


            if (!response.IsSuccessStatusCode)
            {
                throw new PaymentGatewayException(
                    $"PayPal error {(int)response.StatusCode}: {responseContent}");
            }

            var responseDto = JsonSerializer.Deserialize<PayPalOrderResponseDto>(
                      responseContent, JsonOptions) ?? throw new PaymentGatewayException("Empty PayPal response.");
            var approveLink = responseDto.Links.FirstOrDefault(l => l.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase));

            if (approveLink == null)
            {
                throw new PaymentGatewayException("PayPal approval link not found in response.");
            }

            return new PaymentCreated()
            {
                Id = responseDto.Id,
                Status = responseDto.Status,
                ApprovalUrl = new Uri(approveLink.Href)
            };

        }

        public async Task<PaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Get, PayPalEndpoints.GetOrder(paymentId));

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new PaymentGatewayException(
                    $"PayPal error {(int)response.StatusCode}: {responseContent}");
            }

            var responseDto = JsonSerializer.Deserialize<PayPalOrderResponseDto>(
                          responseContent, JsonOptions) ?? throw new PaymentGatewayException("Empty PayPal response.");

            var unit = responseDto.PurchaseUnits.FirstOrDefault();
            if (unit == null)
            {
                throw new PaymentGatewayException("PayPal order has no purchase unit.");
            }

            var amount = decimal.Parse(unit.Amount.Value, CultureInfo.InvariantCulture);

            return new PaymentDetails
            {
                Id = responseDto.Id,
                Status = responseDto.Status,
                Amount = amount,
                Currency = unit.Amount.CurrencyCode,
                CreatedAt = responseDto.CreateTime
            };
        }


        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            // In production would cache this token until expiry.
            var credentials = $"{_payPalOptions.ClientId}:{_payPalOptions.ClientSecret}";
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            using var request = new HttpRequestMessage(
                HttpMethod.Post, PayPalEndpoints.GetAccessToken)
            {
                Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, HttpContentTypes.FormUrlEncoded)
            };

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", authValue);

            using var response = await _httpClient.SendAsync(request, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new PaymentGatewayException(
                    $"PayPal OAuth error {(int)response.StatusCode}: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var tokenElement))
            {
                throw new PaymentGatewayException("PayPal OAuth response has no access_token.");
            }

            return tokenElement.GetString()!;
        }
    }
}
