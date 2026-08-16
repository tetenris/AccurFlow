using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class ItemEntity : BaseEntity
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Inventory";
        public Guid? ItemGroupId { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? InventoryAccountId { get; set; }
        public Guid? SalesAccountId { get; set; }
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public ItemGroupEntity? ItemGroup { get; set; }
        public ChartOfAccountEntity? InventoryAccount { get; set; }
        public ChartOfAccountEntity? SalesAccount { get; set; }
        public ChartOfAccountEntity? CostOfGoodsSoldAccount { get; set; }
    }

    public class ItemGroupEntity : BaseEntity
    {
        public Guid ItemGroupId { get; set; }
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UnitEntity : BaseEntity
    {
        public Guid UnitId { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
