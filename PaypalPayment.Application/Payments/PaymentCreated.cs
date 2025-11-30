using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaypalPayment.Application.Payments
{
    // Result after creating a payment at Mollie
    public class PaymentCreated
    {
        public string Id { get; init; } = default!;
        public string Status { get; init; } = default!;
        public Uri CheckoutUrl { get; init; } = default!;
    }
}
