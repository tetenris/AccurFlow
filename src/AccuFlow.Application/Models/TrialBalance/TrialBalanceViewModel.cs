namespace AccuFlow.Models.TrialBalance;

public class TrialBalanceViewModel
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceGroupViewModel> Groups { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Difference { get; set; }
    public bool IsBalanced { get; set; }
}
