using PaypalPayment.Application.Payments;
using PaypalPayment.Domain.Payments;

namespace PaypalPayment.Application.Usecases.Payments
{
    public class CreatePayment
    {
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentRepository  _paymentRepository;
        public CreatePayment(IPaymentGateway paymentGateway, IPaymentRepository paymentRepository)
        {
            _paymentGateway = paymentGateway;
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentCreated> ExecuteAsync(PaymentRequest request, CancellationToken ct = default)
        {
            var createdRequest = await _paymentGateway.CreatePaymentAsync(request, ct);
            var payment= new Payment
            {
                ProviderOrderId = createdRequest.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = PaymentStatus.Created,
                CreatedAt = DateTime.UtcNow,
                Description = request.Description,
            };

            await _paymentRepository.AddAsync(payment, ct);

            return createdRequest;
        }       
    }
}



