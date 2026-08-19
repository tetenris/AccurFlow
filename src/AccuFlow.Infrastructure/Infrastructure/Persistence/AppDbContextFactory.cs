using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AccuFlow.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory so EF Core tools (dotnet ef) can build AppDbContext
    /// when generating/applying migrations outside the web host (e.g. dotnet ef database update).
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string dbProvider = configuration["Database:Provider"] ?? "SqlServer";
            string connectionString = configuration.GetConnectionString("DefaultConnection")!;

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            switch (dbProvider.ToLowerInvariant())
            {
                case "postgres":
                case "postgresql":
                case "npgsql":
                    optionsBuilder.UseNpgsql(connectionString);
                    break;
                case "mysql":
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    break;
                default:
                    optionsBuilder.UseSqlServer(connectionString);
                    break;
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}