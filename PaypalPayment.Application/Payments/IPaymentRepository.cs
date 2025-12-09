using PaypalPayment.Domain.Payments;

namespace PaypalPayment.Application.Payments
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken ct = default);
        Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId, CancellationToken ct = default);
        Task UpdateAsync(Payment payment, CancellationToken ct = default);
    }
}
