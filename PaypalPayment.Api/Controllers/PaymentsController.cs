using Microsoft.AspNetCore.Mvc;
using PaypalPayment.Application.Payments;
using PaypalPayment.Application.Usecases.Payments;

namespace PaypalPayment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → /api/payments
    public class PaymentsController : ControllerBase
    {
        private readonly CreatePayment _createPayment;
        private readonly GetPayment _getPayment;
        private readonly CapturePayment _capturePayment;

        public PaymentsController(CreatePayment createPayment, GetPayment getPayment, CapturePayment capturePayment)
        {
            _createPayment = createPayment;
            _getPayment = getPayment;
            _capturePayment = capturePayment;
        }

        // POST /api/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest paymentRequest, CancellationToken ct)
        {
            try
            {
                var createdRequest = await _createPayment.ExecuteAsync(paymentRequest, ct);

                // 201 with Location header
                return CreatedAtAction(nameof(GetPaymentById), new { id = createdRequest.Id }, createdRequest);
            }
            catch (PaymentGatewayException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        }


        // GET /api/payments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(string id, CancellationToken ct)
        {
            try
            {
                var payment = await _getPayment.ExecuteAsync(id, ct);

                if (payment is null)
                {
                    return NotFound();
                }

                return Ok(payment);
            }
            catch (PaymentGatewayException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        }

        // POST /api/payments/{id}/capture
        [HttpPost("{id}/capture")]
        public async Task<IActionResult> CapturePaymentById(string id, CancellationToken ct)
        {
            try
            {
                var result = await _capturePayment.ExecuteAsync(id, ct);
                return Ok(result);
            }
            catch (PaymentGatewayException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
            }
        }
    }
}
