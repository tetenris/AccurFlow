namespace KomatsuERP.Models.TrialBalance;

public class TrialBalanceGroupViewModel
{
    public string AccountType { get; set; } = string.Empty;
    public List<TrialBalanceLineViewModel> Accounts { get; set; } = new();
    public decimal SubtotalDebit { get; set; }
    public decimal SubtotalCredit { get; set; }
}
