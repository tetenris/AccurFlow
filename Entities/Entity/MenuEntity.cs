using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class MenuEntity : BaseEntity
    {
        public Guid MenuId { get; set; }
        public Guid? MenuParentId { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // JSON array: ["view","add","edit","delete"]
        public int Sequence { get; set; }

        // Navigation properties
        public MenuEntity? ParentMenu { get; set; }
        public ICollection<MenuEntity> ChildMenus { get; set; } = new List<MenuEntity>();
    }
}
