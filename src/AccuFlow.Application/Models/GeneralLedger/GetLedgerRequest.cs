namespace AccuFlow.Models.GeneralLedger;

public class GetLedgerRequest
{
    public Guid AccountId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
