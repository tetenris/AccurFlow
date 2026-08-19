using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Features.Roles.Queries;
using AccuFlow.Entities.Enums;
using AccuFlow.Entities.Enums.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class GetRoleDropdownQueryHandler : IRequestHandler<GetRoleDropdownQuery, List<SelectListItem>>
    {
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetRoleDropdownQueryHandler(ISuperAdminCheck superAdminCheck)
        {
            _superAdminCheck = superAdminCheck;
        }

        public async Task<List<SelectListItem>> Handle(GetRoleDropdownQuery request, CancellationToken cancellationToken)
        {
            var roles = EnumExtention.ToSelectList<RoleEnum>();
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            return isSuperAdministrator
                ? roles
                : roles.Where(x => x.Value != ((int)RoleEnum.SuperAdministrator).ToString()).ToList();
        }
    }
}