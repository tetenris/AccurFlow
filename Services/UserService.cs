using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Entities.Enums;
using AccuFlow.Infrastructures;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Services
{
    public interface IUserService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableUserRequest request);
        Task<UserViewModel?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateUserViewModel model);
        Task UpdateAsync(UpdateUserViewModel model);
        Task DeleteAsync(Guid id);
    }

    public class UserService : BaseService, IUserService
    {
        private readonly ICurrentUserService _currentUserService;

        public UserService(AppDbContext dbContext, ICurrentUserService currentUserService) : base(dbContext)
        {
            _currentUserService = currentUserService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableUserRequest request)
        {
            var isSuperAdministrator = await IsCurrentUserSuperAdministratorAsync();
            var query = _dbContext.Set<UserEntity>()
                .Where(x => !x.IsDeleted)
                .Include(u => u.Role)
                .Where(x => isSuperAdministrator || x.Role == null || x.Role.RoleType != RoleEnum.SuperAdministrator)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(request.Search) ||
                    u.Email.Contains(request.Search) ||
                    u.FullName.Contains(request.Search) ||
                    (u.Role != null && u.Role.RoleName.Contains(request.Search))
                );
            }

            // Filter by status
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            // Filter by role
            if (request.RoleId.HasValue)
            {
                query = query.Where(x => x.RoleId == request.RoleId.Value);
            }

            // Total records
            var totalRecords = await query.CountAsync();

            // Ordering
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                var orderBy = request.OrderBy == "roleName" ? "Role.RoleName" : request.OrderBy;
                query = query.OrderBy($"{orderBy} {request.OrderType}");
            }

            // Paging
            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    CreatedBy = u.CreatedBy,
                    CreatedAt = u.CreatedAt,
                    UpdatedBy = u.UpdatedBy,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = data
            };
        }

        public async Task<UserViewModel?> GetByIdAsync(Guid id)
        {
            var isSuperAdministrator = await IsCurrentUserSuperAdministratorAsync();
            var user = await _dbContext.Set<UserEntity>()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted && (isSuperAdministrator || u.Role == null || u.Role.RoleType != RoleEnum.SuperAdministrator));

            if (user == null) return null;

            return new UserViewModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "",
                CreatedBy = user.CreatedBy,
                CreatedAt = user.CreatedAt,
                UpdatedBy = user.UpdatedBy,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task CreateAsync(CreateUserViewModel model)
        {
            // Check if email already exists
            var existingUser = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }

            // Check if username already exists
            var existingUsername = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.UserName == model.UserName && !u.IsDeleted);

            if (existingUsername != null)
            {
                throw new Exception("Username already exists");
            }

            // Check if role exists
            var roleExists = await _dbContext.Set<RoleEntity>().AnyAsync(r => r.RoleId == model.RoleId && !r.IsDeleted);
            if (!roleExists)
            {
                throw new Exception("Selected role does not exist");
            }

            await EnsureCanAssignRoleAsync(model.RoleId);

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FullName = model.FullName,
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                CreatedBy = _currentUserService.UserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<UserEntity>().Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateUserViewModel model)
        {
            var user = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.UserId == model.UserId && !u.IsDeleted);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var currentRole = await _dbContext.Set<RoleEntity>().FirstOrDefaultAsync(r => r.RoleId == user.RoleId && !r.IsDeleted);
            if (currentRole?.RoleType == RoleEnum.SuperAdministrator && !await IsCurrentUserSuperAdministratorAsync())
            {
                throw new Exception("Super Administrator user cannot be edited from User Management");
            }

            // Check if email already exists (excluding current user)
            var existingUser = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.UserId != model.UserId && !u.IsDeleted);

            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }

            // Check if username already exists (excluding current user)
            var existingUsername = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.UserId != model.UserId && !u.IsDeleted);

            if (existingUsername != null)
            {
                throw new Exception("Username already exists");
            }

            // Check if role exists
            var roleExists = await _dbContext.Set<RoleEntity>().AnyAsync(r => r.RoleId == model.RoleId && !r.IsDeleted);
            if (!roleExists)
            {
                throw new Exception("Selected role does not exist");
            }

            await EnsureCanAssignRoleAsync(model.RoleId);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            user.UpdatedBy = _currentUserService.UserId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            // Password tidak bisa diubah di menu user management
            // Password hanya bisa diubah melalui menu change password

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _dbContext.Set<UserEntity>()
                .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var currentRole = await _dbContext.Set<RoleEntity>().FirstOrDefaultAsync(r => r.RoleId == user.RoleId && !r.IsDeleted);
            if (currentRole?.RoleType == RoleEnum.SuperAdministrator && !await IsCurrentUserSuperAdministratorAsync())
            {
                throw new Exception("Super Administrator user cannot be deleted from User Management");
            }

            // Soft delete
            user.IsDeleted = true;
            user.DeletedBy = _currentUserService.UserId.ToString();
            user.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private async Task EnsureCanAssignRoleAsync(Guid roleId)
        {
            var targetRole = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted);

            if (targetRole?.RoleType == RoleEnum.SuperAdministrator && !await IsCurrentUserSuperAdministratorAsync())
            {
                throw new Exception("Only Super Administrator can assign Super Administrator role");
            }
        }

        private async Task<bool> IsCurrentUserSuperAdministratorAsync()
        {
            if (_currentUserService.RoleId == Guid.Empty)
            {
                return false;
            }

            return await _dbContext.Set<RoleEntity>()
                .AnyAsync(x => x.RoleId == _currentUserService.RoleId
                    && x.RoleType == RoleEnum.SuperAdministrator
                    && x.IsActive
                    && !x.IsDeleted);
        }
    }
}
