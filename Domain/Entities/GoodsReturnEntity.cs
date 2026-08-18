using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class GoodsReturnEntity : BaseEntity
    {
        public Guid GoodsReturnId { get; set; }
        public string GoodsReturnNumber { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "Sales";
        public DateTime ReturnDate { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid WarehouseId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? JournalId { get; set; }
        public string? Notes { get; set; }
        public InvoiceEntity Invoice { get; set; } = null!;
        public CustomerEntity? Customer { get; set; }
        public SupplierEntity? Supplier { get; set; }
        public WarehouseEntity Warehouse { get; set; } = null!;
        public JournalEntryEntity? JournalEntry { get; set; }
        public ICollection<GoodsReturnLineEntity> Lines { get; set; } = new List<GoodsReturnLineEntity>();
    }
}
