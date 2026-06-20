using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class ItemEntity : BaseEntity
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Inventory";
        public string Unit { get; set; } = "PCS";
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? InventoryAccountId { get; set; }
        public Guid? SalesAccountId { get; set; }
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public ChartOfAccountEntity? InventoryAccount { get; set; }
        public ChartOfAccountEntity? SalesAccount { get; set; }
        public ChartOfAccountEntity? CostOfGoodsSoldAccount { get; set; }
    }
}
