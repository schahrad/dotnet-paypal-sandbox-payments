using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaypalPayment.Application.Payments;
using PaypalPayment.Infrastructure.PayPal;
using Microsoft.Extensions.Options;



namespace PaypalPayment.Infrastructure.DependencyInjection
{

    //This fully wires up PayPalPaymentGateway → HttpClient + configuration.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PayPalOptions>(configuration.GetSection("PayPal"));

            services.AddHttpClient<IPaymentGateway, PayPalPaymentGateway>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PayPalOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            // EF Core will be added here later (DbContext + repositories)

            return services;
        }
    }
}

