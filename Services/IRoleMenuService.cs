using AccuFlow.Models.RoleMenu;
using AccuFlow.Services;

namespace KomatsuERP.Services;

public interface IRoleMenuService : IBaseService
{
    Task<RoleMenuViewModel> GetRoleMenuPermissionsAsync(Guid roleId);
    Task SaveRoleMenuPermissionsAsync(SaveRoleMenuRequest request);
}
