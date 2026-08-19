using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Role;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class GetRolesDatatableQueryHandler : IRequestHandler<GetRolesDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetRolesDatatableQueryHandler(
            IRepository<RoleEntity> roleRepository,
            ISuperAdminCheck superAdminCheck)
        {
            _roleRepository = roleRepository;
            _superAdminCheck = superAdminCheck;
        }

        public async Task<BaseDatatableResponse> Handle(GetRolesDatatableQuery request, CancellationToken cancellationToken)
        {
            var dataTableRequest = request.Request;
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            var query = _roleRepository.Query()
                .Where(x => !x.IsDeleted && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator));

            var totalRecord = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(dataTableRequest.Search))
            {
                var search = dataTableRequest.Search.ToLower();
                query = query.Where(x =>
                    x.RoleName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search))
                );
            }

            if (dataTableRequest.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == dataTableRequest.IsActive.Value);
            }

            query = dataTableRequest.OrderBy?.ToLower() switch
            {
                "rolename" => dataTableRequest.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.RoleName)
                    : query.OrderByDescending(x => x.RoleName),
                "isactive" => dataTableRequest.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.IsActive)
                    : query.OrderByDescending(x => x.IsActive),
                "createdat" => dataTableRequest.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CreatedAt)
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalFiltered = await query.CountAsync(cancellationToken);

            var data = await query
                .Skip((dataTableRequest.Page - 1) * dataTableRequest.Size)
                .Take(dataTableRequest.Size)
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
                .ToListAsync(cancellationToken);

            return new BaseDatatableResponse
            {
                Draw = dataTableRequest.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }
    }
}