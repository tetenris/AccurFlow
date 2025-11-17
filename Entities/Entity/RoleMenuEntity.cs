using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class RoleMenuEntity : BaseEntity, IEntity
    {
        public Guid RoleMenuId { get; set; } = Guid.NewGuid();
        public Guid RoleId { get; set; }
        public Guid MenuId { get; set; }
        public bool CanView { get; set; } = false;
        public bool CanAdd { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanPost { get; set; } = false;
        public bool CanReverse { get; set; } = false;

        // Navigation properties
        public virtual RoleEntity? Role { get; set; }
        public virtual MenuEntity? Menu { get; set; }
    }
}
