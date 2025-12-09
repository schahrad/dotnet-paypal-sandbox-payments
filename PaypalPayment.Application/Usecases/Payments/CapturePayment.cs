using PaypalPayment.Application.Payments;
using PaypalPayment.Domain.Payments;

namespace PaypalPayment.Application.Usecases.Payments
{
    public class CapturePayment
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentRepository _paymentRepository;
        public CapturePayment(IPaymentGateway paymentGateway, IPaymentRepository paymentRepository)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDetails> ExecuteAsync(string paymentId, CancellationToken ct = default)
        {
            var paymentDetails = await _paymentGateway.CapturePaymentAsync(paymentId, ct);
            var payment = await _paymentRepository.GetByProviderOrderIdAsync(paymentId, ct);

            if (payment != null)
            {
                payment.Amount = paymentDetails.Amount;
                payment.Currency = paymentDetails.Currency;
                payment.CreatedAt = DateTime.UtcNow;
                payment.Status = MapToDomainStatus(paymentDetails.Status);

                await _paymentRepository.UpdateAsync(payment, ct);
            }

            return paymentDetails;
        }
        private static PaymentStatus MapToDomainStatus(string providerStatus) =>
           providerStatus.ToUpperInvariant() switch
           {
               "CREATED" => PaymentStatus.Created,
               "APPROVED" => PaymentStatus.Approved,
               "COMPLETED" => PaymentStatus.Completed,
               _ => PaymentStatus.Failed

           };
    }
}




