using Microsoft.Extensions.DependencyInjection;
using PaypalPayment.Application.Usecases.Payments;

namespace PaypalPayment.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CreatePayment>();
            services.AddScoped<GetPayment>();

            return services;
        }
    }
}
