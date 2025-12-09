using Microsoft.EntityFrameworkCore;
using PaypalPayment.Application.Payments;
using PaypalPayment.Domain.Payments;
using PaypalPayment.Infrastructure.Persistence;

namespace PaypalPayment.Infrastructure.Payments
{
    public class EfPaymentRepository : IPaymentRepository
    {
        private readonly PypalPaymentDbContext _dbContext;
        public EfPaymentRepository(PypalPaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(Payment payment, CancellationToken ct = default)
        {
            await _dbContext.Payments.AddAsync(payment, ct);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId, CancellationToken ct = default)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.ProviderOrderId == providerOrderId, ct);
        }

        public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
        {
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
