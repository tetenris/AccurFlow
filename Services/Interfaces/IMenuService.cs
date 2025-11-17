using AccuFlow.Models.Menu;

namespace AccuFlow.Services.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuViewModel>> GetMenuHierarchyAsync();
        Task<List<MenuViewModel>> GetMenuHierarchyByRoleAsync(Guid roleId);
    }
}
