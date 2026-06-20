using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class PurchaseOrderLineEntity : BaseEntity
    {
        public Guid PurchaseOrderLineId { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public PurchaseOrderEntity PurchaseOrder { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}
