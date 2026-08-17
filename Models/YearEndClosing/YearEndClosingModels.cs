namespace AccuFlow.Models.YearEndClosing
{
    public class YearEndClosingRequest
    {
        public int FiscalYear { get; set; } = DateTime.Today.Year;
    }

    public class YearEndClosingPreview
    {
        public int FiscalYear { get; set; }
        public DateTime ClosingDate { get; set; }
        public List<YearEndClosingLinePreview> RevenueAccounts { get; set; } = new();
        public List<YearEndClosingLinePreview> ExpenseAccounts { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetIncome { get; set; }
    }

    public class YearEndClosingLinePreview
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class YearEndClosingHistoryItem
    {
        public Guid ClosingId { get; set; }
        public int FiscalYear { get; set; }
        public DateTime ClosingDate { get; set; }
        public Guid? ClosingJournalId { get; set; }
        public string? JournalNumber { get; set; }
    }
}