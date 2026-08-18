using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Entities.Enums.Extensions;
using AccuFlow.Infrastructures;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Role;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Services
{
    public interface IRoleService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request);
        Task<RoleViewModel?> GetById(Guid roleId);
        Task<Guid> Create(CreateRoleRequest request, Guid userId);
        Task Edit(EditRoleRequest request, Guid userId);
        Task Delete(Guid roleId);
        List<SelectListItem> GetRoleDropdown();
        Task<List<RoleViewModel>> GetActiveRolesAsync();
        Task FixRoleTypeData();
    }

    public class RoleService : BaseService, IRoleService
    {
        private readonly ICurrentUserService _currentUserService;

        public RoleService(AppDbContext dbContext, ICurrentUserService currentUserService) : base(dbContext)
        {
            _currentUserService = currentUserService;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request)
        {
            var isSuperAdministrator = await IsCurrentUserSuperAdministratorAsync();
            var query = _dbContext.Set<RoleEntity>()
                .Where(x => !x.IsDeleted && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator))
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => 
                    x.RoleName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search))
                );
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            query = request?.OrderBy?.ToLower() switch
            {
                "rolename" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.RoleName) 
                    : query.OrderByDescending(x => x.RoleName),
                "isactive" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.IsActive) 
                    : query.OrderByDescending(x => x.IsActive),
                "createdat" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.CreatedAt) 
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalFiltered = await query.CountAsync();

            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleType = (int)x.RoleType,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }

        public async Task<RoleViewModel?> GetById(Guid roleId)
        {
            var isSuperAdministrator = await IsCurrentUserSuperAdministratorAsync();

            return await _dbContext.Set<RoleEntity>()
                .Where(x => x.RoleId == roleId && !x.IsDeleted && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator))
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleType = (int)x.RoleType,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> Create(CreateRoleRequest request, Guid userId)
        {
            // Check if role type already exists
            var existingRole = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleType == (RoleEnum)request.RoleType && !x.IsDeleted);
            
            if (existingRole != null)
            {
                throw new Exception($"Role type '{((RoleEnum)request.RoleType).ToString()}' already exists");
            }

            var role = new RoleEntity
            {
                RoleId = Guid.NewGuid(),
                RoleType = (RoleEnum)request.RoleType,
                RoleName = request.RoleName,
                Description = request.Description ?? string.Empty,
                IsActive = request.IsActive,
                Permissions = "[]"
            };
            
            role.CreatedBy = userId.ToString();
            role.CreatedAt = DateTime.UtcNow;

            _dbContext.Set<RoleEntity>().Add(role);
            await _dbContext.SaveChangesAsync();

            return role.RoleId;
        }

        public async Task Edit(EditRoleRequest request, Guid userId)
        {
            var role = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && !x.IsDeleted);

            if (role == null)
                throw new Exception("Role not found");

            if (role.RoleType == RoleEnum.SuperAdministrator)
                throw new Exception("Super Administrator role cannot be edited from Role Management");

            // Check if there are active users using this role
            var activeUsersWithRole = await _dbContext.Set<UserEntity>()
                .Where(u => u.RoleId == request.RoleId && !u.IsDeleted && u.IsActive)
                .ToListAsync();

            // Validate RoleType change
            if (role.RoleType != (RoleEnum)request.RoleType)
            {
                // Cannot change RoleType if there are users using this role
                if (activeUsersWithRole.Any())
                {
                    throw new Exception($"Cannot change role type. There are {activeUsersWithRole.Count} active user(s) assigned to this role. Please reassign these users first.");
                }

                // If role type is changing, check if new type already exists
                var existingRole = await _dbContext.Set<RoleEntity>()
                    .FirstOrDefaultAsync(x => x.RoleType == (RoleEnum)request.RoleType 
                        && x.RoleId != request.RoleId 
                        && !x.IsDeleted);
                
                if (existingRole != null)
                {
                    throw new Exception($"Role type '{((RoleEnum)request.RoleType).ToString()}' already exists");
                }
                
                role.RoleType = (RoleEnum)request.RoleType;
                role.RoleName = request.RoleName;
            }

            // Validate IsActive change (from Active to Inactive)
            if (role.IsActive && !request.IsActive)
            {
                // Cannot deactivate role if there are active users using it
                if (activeUsersWithRole.Any())
                {
                    var userNames = string.Join(", ", activeUsersWithRole.Select(u => u.UserName).Take(5));
                    var moreUsers = activeUsersWithRole.Count > 5 ? $" and {activeUsersWithRole.Count - 5} more" : "";
                    throw new Exception($"Cannot deactivate role. There are {activeUsersWithRole.Count} active user(s) assigned to this role ({userNames}{moreUsers}). Please reassign or deactivate these users first.");
                }
            }
            
            // Update role properties
            role.Description = request.Description ?? string.Empty;
            role.IsActive = request.IsActive;
            
            var userIdStr = userId.ToString();
            role.UpdatedBy = userIdStr;
            role.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid roleId)
        {
            var role = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleId == roleId && !x.IsDeleted);

            if (role == null)
                throw new Exception("Role not found");

            if (role.RoleType == RoleEnum.SuperAdministrator)
                throw new Exception("Super Administrator role cannot be deleted from Role Management");

            // Check if role is still being used by any users
            var usersWithRole = await _dbContext.Set<UserEntity>()
                .Where(u => u.RoleId == roleId && !u.IsDeleted)
                .CountAsync();

            if (usersWithRole > 0)
            {
                throw new Exception($"Cannot delete role. There are {usersWithRole} user(s) still assigned to this role. Please reassign or remove these users first.");
            }

            // Auto-delete related RoleMenu records (cascade delete)
            var roleMenus = await _dbContext.Set<RoleMenuEntity>()
                .Where(rm => rm.RoleId == roleId && !rm.IsDeleted)
                .ToListAsync();

            if (roleMenus.Any())
            {
                foreach (var roleMenu in roleMenus)
                {
                    roleMenu.IsDeleted = true;
                    roleMenu.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Soft delete the role
            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public List<SelectListItem> GetRoleDropdown()
        {
            var roles = EnumExtention.ToSelectList<RoleEnum>();
            return IsCurrentUserSuperAdministrator()
                ? roles
                : roles.Where(x => x.Value != ((int)RoleEnum.SuperAdministrator).ToString()).ToList();
        }

        public async Task<List<RoleViewModel>> GetActiveRolesAsync()
        {
            var isSuperAdministrator = await IsCurrentUserSuperAdministratorAsync();

            return await _dbContext.Set<RoleEntity>()
                .Where(x => x.IsActive && !x.IsDeleted && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator))
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleType = (int)x.RoleType,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task FixRoleTypeData()
        {
            var roles = await _dbContext.Set<RoleEntity>()
                .Where(x => !x.IsDeleted && x.RoleType == 0)
                .ToListAsync();

            foreach (var role in roles)
            {
                var roleName = role.RoleName.ToLower();
                if (roleName.Contains("admin"))
                    role.RoleType = RoleEnum.Administrator;
                else if (roleName.Contains("accountant"))
                    role.RoleType = RoleEnum.Accountant;
                else if (roleName.Contains("manager"))
                    role.RoleType = RoleEnum.Manager;
                else if (roleName.Contains("finance"))
                    role.RoleType = RoleEnum.FinanceStaff;
                else if (roleName.Contains("ar"))
                    role.RoleType = RoleEnum.AROfficer;
                else if (roleName.Contains("ap"))
                    role.RoleType = RoleEnum.APOfficer;
                else if (roleName.Contains("purchasing"))
                    role.RoleType = RoleEnum.Purchasing;
                else if (roleName.Contains("sales"))
                    role.RoleType = RoleEnum.Sales;
                else if (roleName.Contains("warehouse"))
                    role.RoleType = RoleEnum.Warehouse;
                else if (roleName.Contains("viewer") || roleName.Contains("user"))
                    role.RoleType = RoleEnum.Viewer;
            }

            await _dbContext.SaveChangesAsync();
        }

        private bool IsCurrentUserSuperAdministrator()
        {
            return string.Equals(_currentUserService.RoleName, "Super Administrator", StringComparison.OrdinalIgnoreCase);
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


