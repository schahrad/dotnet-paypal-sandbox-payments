using PaypalPayment.Application.Payments;

namespace PaypalPayment.Application.Usecases.Payments
{
    public class CreatePayment
    {
        private readonly IPaymentGateway _paymentGateway;
        public CreatePayment(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public Task<PaymentCreated> ExecuteAsync(PaymentRequest request, CancellationToken ct = default)
            => _paymentGateway.CreatePaymentAsync(request, ct);
    }
}
