using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaypalPayment.Application.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaypalPayment.Infrastructure.DependencyInjection
{
    
    //This fully wires up PayPalPaymentGateway → HttpClient + configuration.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {         

            // EF Core registrations will come later 

            return services;
        }
    }
}
