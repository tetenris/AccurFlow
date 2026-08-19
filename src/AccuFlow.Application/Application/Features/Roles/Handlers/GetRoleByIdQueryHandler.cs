using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.Role;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleViewModel?>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetRoleByIdQueryHandler(
            IRepository<RoleEntity> roleRepository,
            ISuperAdminCheck superAdminCheck)
        {
            _roleRepository = roleRepository;
            _superAdminCheck = superAdminCheck;
        }

        public async Task<RoleViewModel?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            return await _roleRepository.Query()
                .Where(x => x.RoleId == request.RoleId
                    && !x.IsDeleted
                    && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator))
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
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}