using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.User;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class GetUsersDatatableQueryHandler : IRequestHandler<GetUsersDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetUsersDatatableQueryHandler(
            IRepository<UserEntity> userRepository,
            ISuperAdminCheck superAdminCheck)
        {
            _userRepository = userRepository;
            _superAdminCheck = superAdminCheck;
        }

        public async Task<BaseDatatableResponse> Handle(GetUsersDatatableQuery request, CancellationToken cancellationToken)
        {
            var dataTableRequest = request.Request;
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            var query = _userRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(u => u.Role)
                .Where(x => isSuperAdministrator || x.Role == null || x.Role.RoleType != RoleEnum.SuperAdministrator);

            if (!string.IsNullOrEmpty(dataTableRequest.Search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(dataTableRequest.Search) ||
                    u.Email.Contains(dataTableRequest.Search) ||
                    u.FullName.Contains(dataTableRequest.Search) ||
                    (u.Role != null && u.Role.RoleName.Contains(dataTableRequest.Search))
                );
            }

            if (dataTableRequest.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == dataTableRequest.IsActive.Value);
            }

            if (dataTableRequest.RoleId.HasValue)
            {
                query = query.Where(x => x.RoleId == dataTableRequest.RoleId.Value);
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(dataTableRequest.OrderBy))
            {
                var orderBy = dataTableRequest.OrderBy == "roleName" ? "Role.RoleName" : dataTableRequest.OrderBy;
                query = query.OrderBy($"{orderBy} {dataTableRequest.OrderType}");
            }

            var data = await query
                .Skip((dataTableRequest.Page - 1) * dataTableRequest.Size)
                .Take(dataTableRequest.Size)
                .Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    IsLocked = u.IsLocked,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    PasswordExpiresAt = u.PasswordExpiresAt,
                    LockedAt = u.LockedAt,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : "",
                    CreatedBy = u.CreatedBy,
                    CreatedAt = u.CreatedAt,
                    UpdatedBy = u.UpdatedBy,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new BaseDatatableResponse
            {
                Draw = dataTableRequest.Draw,
                RecordsTotal = totalRecords,
                RecordsFiltered = totalRecords,
                Data = data
            };
        }
    }
}