namespace KomatsuERP.Models.GeneralLedger;

public class LedgerEntryViewModel
{
    public Guid JournalLineId { get; set; }
    public Guid JournalId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime JournalDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public string PostedBy { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReversal { get; set; }
}
