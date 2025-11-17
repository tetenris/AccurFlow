namespace KomatsuERP.Models.JournalEntry;

public class JournalLineViewModel
{
    public Guid JournalLineId { get; set; }
    public int LineNumber { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
