using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class GoodsReceiptEntity : BaseEntity
    {
        public Guid GoodsReceiptId { get; set; }
        public string GoodsReceiptNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid WarehouseId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? JournalId { get; set; }
        public string? Notes { get; set; }
        public PurchaseOrderEntity PurchaseOrder { get; set; } = null!;
        public SupplierEntity Supplier { get; set; } = null!;
        public WarehouseEntity Warehouse { get; set; } = null!;
        public JournalEntryEntity? JournalEntry { get; set; }
        public ICollection<GoodsReceiptLineEntity> Lines { get; set; } = new List<GoodsReceiptLineEntity>();
    }
}