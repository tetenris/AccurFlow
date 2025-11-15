namespace AccuFlow.Models.Role
{
    public class RoleViewModel
    {
        public Guid RoleId { get; set; }
        public int RoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRoleRequest
    {
        public int RoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EditRoleRequest
    {
        public Guid RoleId { get; set; }
        public int RoleType { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
