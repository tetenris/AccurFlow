using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class PurchaseOrderEntity : BaseEntity
    {
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public Guid SupplierId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? PurchaseInvoiceId { get; set; }
        public string? Notes { get; set; }
        public SupplierEntity Supplier { get; set; } = null!;
        public InvoiceEntity? PurchaseInvoice { get; set; }
        public ICollection<PurchaseOrderLineEntity> Lines { get; set; } = new List<PurchaseOrderLineEntity>();
    }
}
