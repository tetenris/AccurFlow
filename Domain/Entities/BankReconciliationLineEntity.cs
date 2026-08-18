using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class BankReconciliationLineEntity : BaseEntity
    {
        public Guid ReconciliationLineId { get; set; } = Guid.NewGuid();
        public Guid ReconciliationId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsCleared { get; set; }

        public BankReconciliationEntity? Reconciliation { get; set; }
    }
}
