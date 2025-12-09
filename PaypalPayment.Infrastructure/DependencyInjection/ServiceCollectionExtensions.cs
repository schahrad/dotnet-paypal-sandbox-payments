using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaypalPayment.Application.Payments;
using PaypalPayment.Infrastructure.PayPal;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using PaypalPayment.Infrastructure.Persistence;
using PaypalPayment.Infrastructure.Payments;



namespace PaypalPayment.Infrastructure.DependencyInjection
{

    //This fully wires up PayPalPaymentGateway → HttpClient + configuration.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PayPalOptions>(configuration.GetSection("PayPal"));

            //PayPal HttpClient + gateway
            services.AddHttpClient<IPaymentGateway, PayPalPaymentGateway>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PayPalOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            // EF Core: DbContext - SQL Server connection
            var connectionString = configuration.GetConnectionString("PaypalPaymentDatabase");
            services.AddDbContext<PypalPaymentDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // Register the PaymentRepository
            services.AddScoped<IPaymentRepository, EfPaymentRepository>();
            return services;
        }
    }
}

