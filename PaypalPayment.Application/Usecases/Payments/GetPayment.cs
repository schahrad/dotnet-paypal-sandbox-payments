using PaypalPayment.Application.Payments;

namespace PaypalPayment.Application.Usecases.Payments
{
    public class GetPayment
    {
        private readonly IPaymentGateway _paymentGateway;
        public GetPayment(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public Task<PaymentDetails?> ExecuteAsync(string id, CancellationToken ct = default)
            => _paymentGateway.GetPaymentAsync(id, ct);
    }
}
