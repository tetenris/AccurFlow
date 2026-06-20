using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class InvoiceEntity : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = "Sales";
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public Guid? JournalId { get; set; }
        public string? Notes { get; set; }
        public CustomerEntity? Customer { get; set; }
        public SupplierEntity? Supplier { get; set; }
        public JournalEntryEntity? JournalEntry { get; set; }
        public ICollection<InvoiceLineEntity> Lines { get; set; } = new List<InvoiceLineEntity>();
    }
}
