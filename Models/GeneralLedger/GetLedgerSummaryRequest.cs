namespace KomatsuERP.Models.GeneralLedger;

public class GetLedgerSummaryRequest
{
    public string? AccountType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
}
