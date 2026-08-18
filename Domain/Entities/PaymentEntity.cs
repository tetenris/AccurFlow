using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class PaymentEntity : BaseEntity
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "Receipt";
        public DateTime PaymentDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid CashBankAccountId { get; set; }
        public string PaymentMethod { get; set; } = "BankTransfer";
        public string Status { get; set; } = "Draft";
        public decimal TotalAmount { get; set; }
        public Guid? JournalId { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public CustomerEntity? Customer { get; set; }
        public SupplierEntity? Supplier { get; set; }
        public ChartOfAccountEntity CashBankAccount { get; set; } = null!;
        public JournalEntryEntity? JournalEntry { get; set; }
        public ICollection<PaymentAllocationEntity> Allocations { get; set; } = new List<PaymentAllocationEntity>();
    }
}

