namespace PaypalPayment.Application.Payments
{
    public class PaymentGatewayException : Exception
    {
        public PaymentGatewayException(string message) : base(message)
        {
        }
    }

    public class PaymentNotFoundException : PaymentGatewayException
    {
        public PaymentNotFoundException(string id)
            : base($"Payment '{id}' was not found.")
        {
        }
    }
}
