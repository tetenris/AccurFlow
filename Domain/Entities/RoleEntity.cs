using AccuFlow.Domain.Common;
using AccuFlow.Entities.Enums;

namespace AccuFlow.Domain.Entities
{
    public class RoleEntity : BaseEntity, IEntity
    {
        public Guid RoleId { get; set; } = Guid.NewGuid();
        public RoleEnum RoleType { get; set; } = RoleEnum.Viewer;
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Permissions { get; set; } = "[]";
        public bool IsActive { get; set; } = true;
    }
}

