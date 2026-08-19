namespace AccuFlow.Models.FinancialStatement;

public class BalanceSheetViewModel
{
    public DateTime AsOfDate { get; set; }
    public List<BalanceSheetSectionViewModel> Sections { get; set; } = new();
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public bool IsBalanced { get; set; }
    public decimal Difference { get; set; }
}

public class BalanceSheetSectionViewModel
{
    public string SectionName { get; set; } = string.Empty;
    public List<BalanceSheetLineViewModel> Lines { get; set; } = new();
    public decimal Subtotal { get; set; }
}

public class BalanceSheetLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class GetBalanceSheetRequest
{
    public DateTime AsOfDate { get; set; } = DateTime.Now;
    public bool ShowZeroBalance { get; set; } = false;
}
