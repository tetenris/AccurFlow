param(
    [string]$TargetPath = "D:\ASP.NET\2025\AKURAT\AccuFlow"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Generating AccuFlow Base Structure" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Create directories
$directories = @(
    "Controllers",
    "Services",
    "Models\BaseModel",
    "Infrastructures",
    "Data"
)

Write-Host "Creating directories..." -ForegroundColor Yellow
foreach ($dir in $directories) {
    $fullPath = Join-Path $TargetPath $dir
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        Write-Host "  Created: $dir" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Generating files..." -ForegroundColor Yellow

# ============================================
# 1. BaseDatatableRequest.cs
# ============================================
$baseDatatableRequest = @'
namespace AccuFlow.Models.BaseModel
{
    public class BaseDatatableRequest
    {
        public int Draw { get; set; }
        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public string? OrderType { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Models\BaseModel\BaseDatatableRequest.cs") -Value $baseDatatableRequest
Write-Host "  Generated: Models\BaseModel\BaseDatatableRequest.cs" -ForegroundColor Green

# ============================================
# 2. BaseDatatableResponse.cs
# ============================================
$baseDatatableResponse = @'
namespace AccuFlow.Models.BaseModel
{
    public class BaseDatatableResponse
    {
        public int? Draw { get; set; } = 0;
        public int? RecordsTotal { get; set; } = 0;
        public int? RecordsFiltered { get; set; } = 0;
        public object? Data { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
        public bool IsSuccess { get; set; } = true;
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Models\BaseModel\BaseDatatableResponse.cs") -Value $baseDatatableResponse
Write-Host "  Generated: Models\BaseModel\BaseDatatableResponse.cs" -ForegroundColor Green

# ============================================
# 3. ICurrentUserService.cs
# ============================================
$currentUserService = @'
namespace AccuFlow.Infrastructures
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string UserName { get; }
        string FullName { get; }
        string Email { get; }
        Guid RoleId { get; }
        string RoleName { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId => GetClaimValue<Guid>("UserId");
        public string UserName => GetClaimValue<string>("UserName") ?? string.Empty;
        public string FullName => GetClaimValue<string>("FullName") ?? string.Empty;
        public string Email => GetClaimValue<string>("Email") ?? string.Empty;
        public Guid RoleId => GetClaimValue<Guid>("RoleId");
        public string RoleName => GetClaimValue<string>("RoleName") ?? string.Empty;

        private T GetClaimValue<T>(string claimType)
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType);
            if (claim == null) return default(T)!;

            try
            {
                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default(T)!;
            }
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Infrastructures\ICurrentUserService.cs") -Value $currentUserService
Write-Host "  Generated: Infrastructures\ICurrentUserService.cs" -ForegroundColor Green

# ============================================
# 4. AppDbContext.cs
# ============================================
$appDbContext = @'
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add your DbSets here
        // Example: public DbSet<YourEntity> YourEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure your entities here
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Data\AppDbContext.cs") -Value $appDbContext
Write-Host "  Generated: Data\AppDbContext.cs" -ForegroundColor Green

# ============================================
# 5. BaseService.cs
# ============================================
$baseService = @'
using AccuFlow.Data;

namespace AccuFlow.Services
{
    public interface IBaseService
    {
    }

    public class BaseService : IBaseService
    {
        protected readonly AppDbContext _dbContext;

        public BaseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Services\BaseService.cs") -Value $baseService
Write-Host "  Generated: Services\BaseService.cs" -ForegroundColor Green

# ============================================
# 6. BaseController.cs
# ============================================
$baseController = @'
using AccuFlow.Infrastructures;
using AccuFlow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccuFlow.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IBaseService _baseService;
        protected ICurrentUserService _currentUserService => HttpContext.RequestServices.GetService<ICurrentUserService>()!;

        public BaseController(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Set user info to ViewBag/TempData
            TempData["UserId"] = _currentUserService.UserId;
            TempData["FullName"] = _currentUserService.FullName;
            TempData["RoleName"] = _currentUserService.RoleName;
            ViewBag.UserName = _currentUserService.UserName;

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Controllers\BaseController.cs") -Value $baseController
Write-Host "  Generated: Controllers\BaseController.cs" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Base Structure Generated Successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files generated:" -ForegroundColor Yellow
Write-Host "  - Models\BaseModel\BaseDatatableRequest.cs" -ForegroundColor White
Write-Host "  - Models\BaseModel\BaseDatatableResponse.cs" -ForegroundColor White
Write-Host "  - Infrastructures\ICurrentUserService.cs" -ForegroundColor White
Write-Host "  - Data\AppDbContext.cs" -ForegroundColor White
Write-Host "  - Services\BaseService.cs" -ForegroundColor White
Write-Host "  - Controllers\BaseController.cs" -ForegroundColor White
Write-Host ""
