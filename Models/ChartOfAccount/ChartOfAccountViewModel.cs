namespace AccuFlow.Models.ChartOfAccount
{
    public class ChartOfAccountViewModel
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentAccountId { get; set; }
        public string? ParentAccountName { get; set; }
        public bool IsHeader { get; set; }
        public bool IsActive { get; set; }
        public decimal OpeningBalance { get; set; }
        public string NormalBalance { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool HasChildren { get; set; }
        public int ChildCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
