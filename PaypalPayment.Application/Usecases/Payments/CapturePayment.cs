using PaypalPayment.Application.Payments;

namespace PaypalPayment.Application.Usecases.Payments
{
    public class CapturePayment
    {
        private readonly IPaymentGateway _paymentGateway;
        public CapturePayment(IPaymentGateway paymentGateway)
        {
                _paymentGateway = paymentGateway;
        }

        public Task<PaymentDetails> ExecuteAsync(string paymentId, CancellationToken ct = default)
            => _paymentGateway.CapturePaymentAsync(paymentId, ct);
    }
}
