namespace AccuFlow.Models.FinancialStatement;

public class IncomeStatementViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<IncomeStatementSectionViewModel> Sections { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetIncome { get; set; }
}

public class IncomeStatementSectionViewModel
{
    public string SectionName { get; set; } = string.Empty;
    public List<IncomeStatementLineViewModel> Lines { get; set; } = new();
    public decimal Subtotal { get; set; }
}

public class IncomeStatementLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class GetIncomeStatementRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool ShowZeroBalance { get; set; } = false;
}
