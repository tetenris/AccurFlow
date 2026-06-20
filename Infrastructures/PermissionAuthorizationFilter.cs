using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using AccuFlow.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Infrastructures
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly AppDbContext _dbContext;

        public PermissionAuthorizationFilter(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action) || IsExcludedController(controller))
            {
                return;
            }

            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var roleIdClaim = context.HttpContext.User.FindFirst("RoleId")?.Value;
            if (!Guid.TryParse(roleIdClaim, out var roleId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var role = await _dbContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == roleId && x.IsActive && !x.IsDeleted);

            if (role == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (role.RoleType == RoleEnum.SuperAdministrator)
            {
                return;
            }

            var menu = await _dbContext.Menus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Controller == controller && !x.IsDeleted);

            if (menu == null)
            {
                return;
            }

            var roleMenu = await _dbContext.RoleMenus
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == roleId && x.MenuId == menu.MenuId && !x.IsDeleted);

            if (roleMenu == null || !HasPermission(roleMenu, action))
            {
                context.Result = new ForbidResult();
            }
        }

        private static bool IsExcludedController(string controller)
        {
            return controller.Equals("Account", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasPermission(RoleMenuEntity roleMenu, string action)
        {
            return MapAction(action) switch
            {
                "view" => roleMenu.CanView,
                "add" => roleMenu.CanAdd,
                "edit" => roleMenu.CanEdit,
                "delete" => roleMenu.CanDelete,
                "post" => roleMenu.CanPost,
                "reverse" => roleMenu.CanReverse,
                _ => roleMenu.CanView
            };
        }

        private static string MapAction(string action)
        {
            if (IsAny(action, "Index", "Datatable", "GetById", "GetRoleDropdown", "GetActiveRoles", "GetActiveCustomers", "GetActiveSuppliers", "GetActiveAccounts", "GetActiveItems", "GetOpenInvoices", "GetByDocument", "Download", "Print", "Generate", "ExportExcel", "StockCard", "StockCardDatatable", "GetRoleMenuPermissions", "GetHierarchy"))
            {
                return "view";
            }

            if (IsAny(action, "Create", "CreateItem", "Upload", "Submit"))
            {
                return "add";
            }

            if (IsAny(action, "Edit", "Update", "UpdateItem", "SaveRoleMenuPermissions", "FixRoleTypeData"))
            {
                return "edit";
            }

            if (IsAny(action, "Delete", "DeleteItem"))
            {
                return "delete";
            }

            if (IsAny(action, "Post", "Approve", "Reject", "Cancel", "ConvertToInvoice"))
            {
                return "post";
            }

            if (IsAny(action, "Reverse"))
            {
                return "reverse";
            }

            return "view";
        }

        private static bool IsAny(string value, params string[] values)
        {
            return values.Any(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
