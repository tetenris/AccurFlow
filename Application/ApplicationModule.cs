using Microsoft.Extensions.DependencyInjection;

namespace AccuFlow.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR will pick up all handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly));

            return services;
        }
    }
}