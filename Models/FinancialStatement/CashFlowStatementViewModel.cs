namespace KomatsuERP.Models.FinancialStatement;

public class CashFlowStatementViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal BeginningCashBalance { get; set; }
    public List<CashFlowSectionViewModel> Sections { get; set; } = new();
    public decimal NetCashFromOperating { get; set; }
    public decimal NetCashFromInvesting { get; set; }
    public decimal NetCashFromFinancing { get; set; }
    public decimal NetIncreaseDecrease { get; set; }
    public decimal EndingCashBalance { get; set; }
}

public class CashFlowSectionViewModel
{
    public string SectionName { get; set; } = string.Empty;
    public List<CashFlowLineViewModel> Lines { get; set; } = new();
    public decimal Subtotal { get; set; }
}

public class CashFlowLineViewModel
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class GetCashFlowStatementRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}
