using AccuFlow.Models.RoleMenu;
using AccuFlow.Services;

namespace AccuFlow.Services;

public interface IRoleMenuService : IBaseService
{
    Task<RoleMenuViewModel> GetRoleMenuPermissionsAsync(Guid roleId);
    Task SaveRoleMenuPermissionsAsync(SaveRoleMenuRequest request);
}
