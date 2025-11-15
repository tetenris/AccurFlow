using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Models.Menu;
using AccuFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccuFlow.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _dbContext;

        public MenuService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MenuViewModel>> GetMenuHierarchyAsync()
        {
            var allMenus = await _dbContext.Menus
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Sequence)
                .ToListAsync();

            var menuViewModels = allMenus.Select(m => new MenuViewModel
            {
                MenuId = m.MenuId,
                MenuParentId = m.MenuParentId,
                Icon = m.Icon,
                Name = m.Name,
                Controller = m.Controller,
                Actions = ParseActions(m.Action),
                Sequence = m.Sequence
            }).ToList();

            // Build hierarchy
            var rootMenus = menuViewModels.Where(m => m.MenuParentId == null).ToList();
            
            foreach (var rootMenu in rootMenus)
            {
                rootMenu.ChildMenus = menuViewModels
                    .Where(m => m.MenuParentId == rootMenu.MenuId)
                    .OrderBy(m => m.Sequence)
                    .ToList();
            }

            return rootMenus;
        }

        private List<string> ParseActions(string actionJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionJson))
                    return new List<string>();

                return JsonSerializer.Deserialize<List<string>>(actionJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
