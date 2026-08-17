using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class GoodsReceiptLineEntity : BaseEntity
    {
        public Guid GoodsReceiptLineId { get; set; }
        public Guid GoodsReceiptId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid PurchaseOrderLineId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public GoodsReceiptEntity GoodsReceipt { get; set; } = null!;
        public ItemEntity? Item { get; set; }
        public PurchaseOrderLineEntity PurchaseOrderLine { get; set; } = null!;
    }
}