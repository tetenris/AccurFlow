using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.Role;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class GetUserRoleDropdownQueryHandler : IRequestHandler<GetUserRoleDropdownQuery, List<RoleViewModel>>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetUserRoleDropdownQueryHandler(
            IRepository<RoleEntity> roleRepository,
            ISuperAdminCheck superAdminCheck)
        {
            _roleRepository = roleRepository;
            _superAdminCheck = superAdminCheck;
        }

        public async Task<List<RoleViewModel>> Handle(GetUserRoleDropdownQuery request, CancellationToken cancellationToken)
        {
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            return await _roleRepository.Query()
                .Where(x => x.IsActive
                    && !x.IsDeleted
                    && (isSuperAdministrator || x.RoleType != RoleEnum.SuperAdministrator))
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleType = (int)x.RoleType,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);
        }
    }
}