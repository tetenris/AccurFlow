using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class ChartOfAccountEntity : BaseEntity, IEntity
    {
        public Guid AccountId { get; set; } = Guid.NewGuid();
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentAccountId { get; set; }
        public bool IsHeader { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public decimal OpeningBalance { get; set; } = 0;
        public string NormalBalance { get; set; } = "Debit";
        public string Currency { get; set; } = "IDR";
        public int Level { get; set; } = 0;
        public int AccountUsage { get; set; } = 0; // 0 = General, 1 = Cash, 2 = Bank (see AccountUsage enum)
        
        // Navigation property for hierarchical structure
        public ChartOfAccountEntity? ParentAccount { get; set; }
        public ICollection<ChartOfAccountEntity> ChildAccounts { get; set; } = new List<ChartOfAccountEntity>();
    }
}

