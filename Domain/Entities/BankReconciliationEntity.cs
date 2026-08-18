using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class BankReconciliationEntity : BaseEntity
    {
        public Guid ReconciliationId { get; set; } = Guid.NewGuid();
        public string ReconciliationNumber { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public DateTime StatementDate { get; set; }
        public decimal StatementEndingBalance { get; set; }
        public decimal GlEndingBalance { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Posted
        public string? Notes { get; set; }
        public DateTime? PostedDate { get; set; }
        public Guid? PostedBy { get; set; }

        public ChartOfAccountEntity? Account { get; set; }
        public ICollection<BankReconciliationLineEntity> Lines { get; set; } = new List<BankReconciliationLineEntity>();
    }
}
