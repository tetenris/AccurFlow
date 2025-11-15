using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Entities.Enums;
using AccuFlow.Entities.Enums.Extensions;
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
        Task Create(CreateRoleRequest request, Guid userId);
        Task Edit(EditRoleRequest request, Guid userId);
        Task Delete(Guid roleId);
        List<SelectListItem> GetRoleDropdown();
        Task<List<RoleViewModel>> GetActiveRolesAsync();
        Task FixRoleTypeData();
    }

    public class RoleService : BaseService, IRoleService
    {
        public RoleService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request)
        {
            var query = _dbContext.Set<RoleEntity>()
                .Where(x => !x.IsDeleted)
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
            return await _dbContext.Set<RoleEntity>()
                .Where(x => x.RoleId == roleId && !x.IsDeleted)
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

        public async Task Create(CreateRoleRequest request, Guid userId)
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
        }

        public async Task Edit(EditRoleRequest request, Guid userId)
        {
            var role = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && !x.IsDeleted);

            if (role == null)
                throw new Exception("Role not found");

            // Check if role type is being changed
            if (role.RoleType != (RoleEnum)request.RoleType)
            {
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
            
            // Always allow updating description and status
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

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public List<SelectListItem> GetRoleDropdown()
        {
            return EnumExtention.ToSelectList<RoleEnum>();
        }

        public async Task<List<RoleViewModel>> GetActiveRolesAsync()
        {
            return await _dbContext.Set<RoleEntity>()
                .Where(x => x.IsActive && !x.IsDeleted)
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
                else if (roleName.Contains("user"))
                    role.RoleType = RoleEnum.User;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
