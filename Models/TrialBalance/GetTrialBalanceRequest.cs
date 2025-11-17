namespace KomatsuERP.Models.TrialBalance;

public class GetTrialBalanceRequest
{
    public DateTime? AsOfDate { get; set; }
    public string? AccountType { get; set; }
    public bool ShowZeroBalance { get; set; } = false;
}
