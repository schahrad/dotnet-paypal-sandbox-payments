namespace PaypalPayment.Application.Payments
{
    public interface IPaymentGateway
    {
        Task<PaymentCreated> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

        Task<PaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
    }
}
