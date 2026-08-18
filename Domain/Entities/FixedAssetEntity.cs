using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class FixedAssetEntity : BaseEntity
    {
        public Guid AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeMonths { get; set; }
        public string DepreciationMethod { get; set; } = "StraightLine";
        public decimal AccumulatedDepreciation { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public string Status { get; set; } = "Active";
        public Guid? AssetAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public string? Notes { get; set; }
        public ChartOfAccountEntity? AssetAccount { get; set; }
        public ChartOfAccountEntity? AccumulatedDepreciationAccount { get; set; }
        public ChartOfAccountEntity? DepreciationExpenseAccount { get; set; }
        public ICollection<FixedAssetDepreciationEntity> Depreciations { get; set; } = new List<FixedAssetDepreciationEntity>();
    }

    public class FixedAssetDepreciationEntity : BaseEntity
    {
        public Guid DepreciationId { get; set; }
        public Guid AssetId { get; set; }
        public DateTime PeriodDate { get; set; }
        public decimal Amount { get; set; }
        public Guid? JournalId { get; set; }
        public FixedAssetEntity Asset { get; set; } = null!;
        public JournalEntryEntity? JournalEntry { get; set; }
    }
}
