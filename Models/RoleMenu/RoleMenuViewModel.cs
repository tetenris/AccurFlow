namespace AccuFlow.Models.RoleMenu;

public class RoleMenuViewModel
{
    public Guid RoleId { get; set; }
    public List<MenuPermissionViewModel> Menus { get; set; } = new();
}

public class MenuPermissionViewModel
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public Guid? ParentMenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPost { get; set; }
    public bool CanReverse { get; set; }
    public List<string> AvailableActions { get; set; } = new();
}

public class SaveRoleMenuRequest
{
    public Guid RoleId { get; set; }
    public List<MenuPermissionDto> Permissions { get; set; } = new();
}

public class MenuPermissionDto
{
    public Guid MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPost { get; set; }
    public bool CanReverse { get; set; }
}
