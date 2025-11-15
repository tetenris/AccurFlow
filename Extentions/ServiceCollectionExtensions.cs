using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;

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
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                    {
                        RequireSsl = false,
                        SslRedirect = false,
                        LoginCaseSensitive = true,
                        Users = new[]
                        {
                            new BasicAuthAuthorizationUser
                            {
                                Login = "admin",
                                PasswordClear = "admin123"
                            }
                        }
                    })
                }
            });

            return app;
        }
    }
}
