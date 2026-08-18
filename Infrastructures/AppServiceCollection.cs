using AccuFlow.Services;

namespace AccuFlow.Infrastructures
{
    public static class AppServiceCollection
    {
        public static IServiceCollection AddAppService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<PermissionAuthorizationFilter>();
            
            // Register base services
            services.AddScoped<IBaseService, BaseService>();

            return services;
        }
    }
}
