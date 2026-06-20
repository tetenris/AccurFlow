using Hangfire;
using Hangfire.Dashboard;

namespace AccuFlow.Extentions
{
    public static class HangfireExtensions
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfireServer();

            return services;
        }

        public static IApplicationBuilder UseHangfireDashboardWithAuth(this IApplicationBuilder app, IConfiguration configuration)
        {
            var requiredRole = configuration.GetValue<string>("Hangfire:Dashboard:RequiredRole") ?? "Administrator";

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new RoleBasedDashboardAuthorizationFilter(requiredRole)
                }
            });

            return app;
        }

        private sealed class RoleBasedDashboardAuthorizationFilter : IDashboardAuthorizationFilter
        {
            private readonly string _requiredRole;

            public RoleBasedDashboardAuthorizationFilter(string requiredRole)
            {
                _requiredRole = requiredRole;
            }

            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();
                return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole(_requiredRole);
            }
        }
    }
}
