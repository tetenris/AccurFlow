# ============================================
# AccuFlow Files Generator
# Generate Controllers, Services, Models, Views, Entities
# ============================================

param(
    [string]$TargetPath = "D:\ASP.NET\2025\AKURAT\AccuFlow",
    [string]$Namespace = "AccuFlow"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  AccuFlow Files Generator" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $TargetPath)) {
    Write-Host "Error: Target path not found: $TargetPath" -ForegroundColor Red
    Write-Host "Please run GenerateAccuFlow.ps1 first!" -ForegroundColor Yellow
    exit
}

Write-Host "Generating files to: $TargetPath" -ForegroundColor Yellow
Write-Host ""

# ============================================
# 1. ENTITIES - Base Classes
# ============================================

Write-Host "Generating Entities..." -ForegroundColor Yellow

# BaseEntity.cs
$baseEntity = @"
using System.ComponentModel.DataAnnotations;

namespace $Namespace.Entities.Abstractions
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        public string? DeletedBy { get; set; }
    }
}
"@

$baseEntity | Out-File -FilePath (Join-Path $TargetPath "Entities\Abstractions\BaseEntity.cs") -Encoding UTF8
Write-Host "  Created: BaseEntity.cs" -ForegroundColor Green

# IEntity.cs
$iEntity = @"
namespace $Namespace.Entities.Abstractions
{
    public interface IEntity
    {
        Guid Id { get; set; }
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        bool IsDeleted { get; set; }
    }
}
"@

$iEntity | Out-File -FilePath (Join-Path $TargetPath "Entities\Abstractions\IEntity.cs") -Encoding UTF8
Write-Host "  Created: IEntity.cs" -ForegroundColor Green

# AppDbContext.cs
$appDbContext = @"
using $Namespace.Entities.Abstractions;
using $Namespace.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace $Namespace.Entities.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets will be added here
        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
"@

$appDbContext | Out-File -FilePath (Join-Path $TargetPath "Entities\Context\AppDbContext.cs") -Encoding UTF8
Write-Host "  Created: AppDbContext.cs" -ForegroundColor Green

# UserEntity.cs
$userEntity = @"
using $Namespace.Entities.Abstractions;

namespace $Namespace.Entities.Entity
{
    public class UserEntity : BaseEntity, IEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
"@

$userEntity | Out-File -FilePath (Join-Path $TargetPath "Entities\Entity\UserEntity.cs") -Encoding UTF8
Write-Host "  Created: UserEntity.cs" -ForegroundColor Green

# ============================================
# 2. SERVICES
# ============================================

Write-Host ""
Write-Host "Generating Services..." -ForegroundColor Yellow

# BaseService.cs
$baseService = @"
using $Namespace.Entities.Context;

namespace $Namespace.Services
{
    public class BaseService
    {
        protected readonly AppDbContext _dbContext;

        public BaseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
"@

$baseService | Out-File -FilePath (Join-Path $TargetPath "Services\BaseService.cs") -Encoding UTF8
Write-Host "  Created: BaseService.cs" -ForegroundColor Green

# AccountService.cs
$accountService = @"
using $Namespace.Entities.Context;
using $Namespace.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace $Namespace.Services
{
    public interface IAccountService
    {
        Task<UserEntity?> ValidateUser(string username, string password);
    }

    public class AccountService : BaseService, IAccountService
    {
        public AccountService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserEntity?> ValidateUser(string username, string password)
        {
            // TODO: Implement proper password hashing
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserName == username && !x.IsDeleted && x.IsActive);

            return user;
        }
    }
}
"@

$accountService | Out-File -FilePath (Join-Path $TargetPath "Services\AccountService.cs") -Encoding UTF8
Write-Host "  Created: AccountService.cs" -ForegroundColor Green

# ============================================
# 3. CONTROLLERS
# ============================================

Write-Host ""
Write-Host "Generating Controllers..." -ForegroundColor Yellow

# BaseController.cs
$baseController = @"
using Microsoft.AspNetCore.Mvc;

namespace $Namespace.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
"@

$baseController | Out-File -FilePath (Join-Path $TargetPath "Controllers\BaseController.cs") -Encoding UTF8
Write-Host "  Created: BaseController.cs" -ForegroundColor Green

# HomeController.cs
$homeController = @"
using Microsoft.AspNetCore.Mvc;

namespace $Namespace.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
"@

$homeController | Out-File -FilePath (Join-Path $TargetPath "Controllers\HomeController.cs") -Encoding UTF8
Write-Host "  Created: HomeController.cs" -ForegroundColor Green

# AccountController.cs
$accountController = @"
using $Namespace.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace $Namespace.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = await _accountService.ValidateUser(username, password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
"@

$accountController | Out-File -FilePath (Join-Path $TargetPath "Controllers\AccountController.cs") -Encoding UTF8
Write-Host "  Created: AccountController.cs" -ForegroundColor Green

# ============================================
# 4. INFRASTRUCTURES
# ============================================

Write-Host ""
Write-Host "Generating Infrastructure..." -ForegroundColor Yellow

# AppServiceCollection.cs
$appServiceCollection = @"
using $Namespace.Services;

namespace $Namespace.Infrastructures
{
    public static class AppServiceCollection
    {
        public static IServiceCollection AddAppService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register services
            services.AddScoped<IAccountService, AccountService>();

            return services;
        }
    }
}
"@

$appServiceCollection | Out-File -FilePath (Join-Path $TargetPath "Infrastructures\AppServiceCollection.cs") -Encoding UTF8
Write-Host "  Created: AppServiceCollection.cs" -ForegroundColor Green

# HangfireExtensions.cs
$hangfireExtensions = @"
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;

namespace $Namespace.Extentions
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
"@

$hangfireExtensions | Out-File -FilePath (Join-Path $TargetPath "Extentions\ServiceCollectionExtensions.cs") -Encoding UTF8
Write-Host "  Created: ServiceCollectionExtensions.cs" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Files Generation Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Generated files:" -ForegroundColor Yellow
Write-Host "  - Entities (BaseEntity, IEntity, AppDbContext, UserEntity)" -ForegroundColor White
Write-Host "  - Services (BaseService, AccountService)" -ForegroundColor White
Write-Host "  - Controllers (BaseController, HomeController, AccountController)" -ForegroundColor White
Write-Host "  - Infrastructure (AppServiceCollection, HangfireExtensions)" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. cd $TargetPath" -ForegroundColor White
Write-Host "2. dotnet restore" -ForegroundColor White
Write-Host "3. dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "4. dotnet ef database update" -ForegroundColor White
Write-Host "5. Create Views (Home/Index.cshtml, Account/Login.cshtml, Shared/_Layout.cshtml)" -ForegroundColor White
Write-Host ""
