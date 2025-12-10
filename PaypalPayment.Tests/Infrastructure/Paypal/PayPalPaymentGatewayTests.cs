using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaypalPayment.Application.Payments;
using PaypalPayment.Infrastructure.PayPal;
using PaypalPayment.Tests.Infrastructure.Http;
using System.Net;
using System.Text;
namespace PaypalPayment.Tests.Infrastructure.PayPal
{
    public class PayPalPaymentGatewayTests
    {
        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            var handler = new FakeHttpMessageHandler(handlerFunc); return new HttpClient(handler)
            { BaseAddress = new Uri("https://api-m.sandbox.paypal.com") };
        }
        private static PayPalPaymentGateway CreateGateway(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            var httpClient = CreateHttpClient(handlerFunc);
            var options = Options.Create(new PayPalOptions
            {
                BaseUrl = "https://api-m.sandbox.paypal.com",
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                ReturnUrl = "https://example.com/return",
                CancelUrl = "https://example.com/cancel"
            });
            var logger = NullLogger<PayPalPaymentGateway>.Instance;
            return new PayPalPaymentGateway(httpClient, options, logger);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnMappedResult_OnSuccess()
        {
            // Arrange: fake responses for token + create order
            var gateway = CreateGateway(request =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/v1/oauth2/token"))
                {
                    var tokenJson = """{ "access_token": "fake-token" }"""; return new HttpResponseMessage(HttpStatusCode.OK)
                    { Content = new StringContent(tokenJson, Encoding.UTF8, "application/json") };
                }
                if (request.RequestUri!.AbsolutePath.EndsWith("/v2/checkout/orders"))
                {
                    var orderJson = """
                     { "id": "TEST_ORDER_ID", "status": "CREATED", "create_time": "2025-12-10T10:00:00Z",
                     "purchase_units": [ { "amount": { "currency_code": "EUR", "value": "10.00" } } ], "links": [ { "href": "https://www.sandbox.paypal.com/checkoutnow?token=TEST_ORDER_ID", "rel": "approve" } ] } 
                    """;

                    return new HttpResponseMessage(HttpStatusCode.Created)

                    { Content = new StringContent(orderJson, Encoding.UTF8, "application/json") };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            var request = new PaymentRequest
            {
                Amount = 10m,
                Currency = "EUR",
                Description = "Test payment"
            };
            // Act
            var result = await gateway.CreatePaymentAsync(request);
            // Assert
            result.Id.Should().Be("TEST_ORDER_ID");
            result.Status.Should().Be("CREATED");
            result.ApprovalUrl.Should().Be(new Uri("https://www.sandbox.paypal.com/checkoutnow?token=TEST_ORDER_ID"));
        }
    }
}