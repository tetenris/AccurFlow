using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class CashBankTransferEntity : BaseEntity
    {
        public Guid TransferId { get; set; } = Guid.NewGuid();
        public string TransferNumber { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Posted
        public string? ReferenceNumber { get; set; }
        public Guid? JournalId { get; set; }
        public DateTime? PostedDate { get; set; }
        public Guid? PostedBy { get; set; }

        public ChartOfAccountEntity? FromAccount { get; set; }
        public ChartOfAccountEntity? ToAccount { get; set; }
        public JournalEntryEntity? JournalEntry { get; set; }
    }
}