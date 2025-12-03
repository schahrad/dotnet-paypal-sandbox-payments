using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PayPalPaymentGateway> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public PayPalPaymentGateway(HttpClient httpClient, IOptions<PayPalOptions> options, ILogger<PayPalPaymentGateway> logger)
        {
            _httpClient = httpClient;
            _payPalOptions = options.Value;
            _logger = logger;
        }
        public async Task<PaymentCreated> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating PayPal order for {Amount} {Currency}", request.Amount, request.Currency);

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
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, PayPalEndpoints.Orders)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, HttpContentTypes.Json)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal create order failed with {StatusCode}: {Content}", (int)response.StatusCode, responseContent);

                throw new PaymentGatewayException($"PayPal error {(int)response.StatusCode}: {responseContent}");
            }

            var responseDto = JsonSerializer.Deserialize<PayPalOrderResponseDto>(
                      responseContent, JsonOptions) ?? throw new PaymentGatewayException("Empty PayPal response.");
            var approveLink = responseDto.Links.FirstOrDefault(l => l.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase));

            if (approveLink == null)
            {
                _logger.LogError("PayPal approval link not found in create order response. OrderId={OrderId}", responseDto.Id);

                throw new PaymentGatewayException("PayPal approval link not found in response.");
            }

            _logger.LogInformation("PayPal order created successfully. OrderId={OrderId}, Status={Status}", responseDto.Id, responseDto.Status);

            return new PaymentCreated()
            {
                Id = responseDto.Id,
                Status = responseDto.Status,
                ApprovalUrl = new Uri(approveLink.Href)
            };
        }

        public async Task<PaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving PayPal order {PaymentId}", paymentId);

            var accessToken = await GetAccessTokenAsync(cancellationToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, PayPalEndpoints.GetOrder(paymentId));

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("PayPal order {PaymentId} not found when retrieving.", paymentId);

                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal get order failed for {PaymentId} with {StatusCode}: {Content}", paymentId,
                    (int)response.StatusCode, responseContent);

                throw new PaymentGatewayException(
                    $"PayPal error {(int)response.StatusCode}: {responseContent}");
            }

            var responseDto = JsonSerializer.Deserialize<PayPalOrderResponseDto>(
                          responseContent, JsonOptions) ?? throw new PaymentGatewayException("Empty PayPal response.");

            var purchaseUnit = responseDto.PurchaseUnits.FirstOrDefault();
            if (purchaseUnit == null)
            {
                _logger.LogError("PayPal order {PaymentId} has no purchase unit in response.", paymentId);

                throw new PaymentGatewayException("PayPal order has no purchase unit.");
            }

            var amount = decimal.Parse(purchaseUnit.Amount.Value, CultureInfo.InvariantCulture);
            _logger.LogInformation("Retrieved PayPal order {PaymentId} with status {Status}", responseDto.Id, responseDto.Status);

            return new PaymentDetails
            {
                Id = responseDto.Id,
                Status = responseDto.Status,
                Amount = amount,
                Currency = purchaseUnit.Amount.CurrencyCode,
                CreatedAt = responseDto.CreateTime
            };
        }

        public async Task<PaymentDetails> CapturePaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Capturing PayPal order {PaymentId}", paymentId);

            var accessToken = await GetAccessTokenAsync(cancellationToken);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, PayPalEndpoints.CaptureOrder(paymentId))
            {
                // PayPal expects JSON, even if body is effectively empty
                Content = new StringContent("{}", Encoding.UTF8, HttpContentTypes.Json)
            };


            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // PayPal does not require a body for a simple capture
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("PayPal order {PaymentId} was not found for capture.", paymentId);

                throw new PaymentGatewayException($"PayPal order '{paymentId}' was not found for capture.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal capture failed for {PaymentId} with {StatusCode}: {Content}", paymentId,
                    (int)response.StatusCode, responseContent);

                throw new PaymentGatewayException($"PayPal capture error {(int)response.StatusCode}: {responseContent}");
            }

            var responseDto = JsonSerializer.Deserialize<PayPalOrderResponseDto>(responseContent, JsonOptions)
                ?? throw new PaymentGatewayException("Empty PayPal capture response.");

            var purchaseUnit = responseDto.PurchaseUnits.FirstOrDefault();
            if (purchaseUnit == null)
            {
                _logger.LogError("PayPal capture response for {PaymentId} has no purchase unit.", paymentId);

                throw new PaymentGatewayException("PayPal capture response has no purchase unit.");
            }

            PayPalAmountDto? amountDto = purchaseUnit.Amount;
            if (amountDto == null || string.IsNullOrWhiteSpace(amountDto.Value))
            {
                amountDto = purchaseUnit.Payments?.Captures?.FirstOrDefault()?.Amount;
            }

            if (amountDto == null || string.IsNullOrWhiteSpace(amountDto.Value))
            {
                _logger.LogError("PayPal capture response for {PaymentId} does not contain an amount.", paymentId);

                throw new PaymentGatewayException("PayPal capture response does not contain amount information.");
            }

            var amount = decimal.Parse(amountDto.Value, CultureInfo.InvariantCulture);
            var currency = amountDto.CurrencyCode;

            _logger.LogInformation("Captured PayPal order {PaymentId} successfully with status {Status}", responseDto.Id, responseDto.Status);

            return new PaymentDetails
            {
                Id = responseDto.Id,
                Status = responseDto.Status,          // should now be COMPLETED
                Amount = amount,
                Currency = currency,
                CreatedAt = responseDto.CreateTime
            };
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            _logger.LogDebug("Requesting PayPal OAuth access token.");

            // In production would cache this token until expiry.
            var credentials = $"{_payPalOptions.ClientId}:{_payPalOptions.ClientSecret}";
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

            using var request = new HttpRequestMessage(
                HttpMethod.Post, PayPalEndpoints.GetAccessToken)
            {
                Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, HttpContentTypes.FormUrlEncoded)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            using var response = await _httpClient.SendAsync(request, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal OAuth error {StatusCode}: {Content}", (int)response.StatusCode, content);

                throw new PaymentGatewayException($"PayPal OAuth error {(int)response.StatusCode}: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var tokenElement))
            {
                _logger.LogError("PayPal OAuth response has no access_token.");

                throw new PaymentGatewayException("PayPal OAuth response has no access_token.");
            }

            _logger.LogDebug("Successfully obtained PayPal OAuth access token.");

            return tokenElement.GetString()!;
        }
    }
}
