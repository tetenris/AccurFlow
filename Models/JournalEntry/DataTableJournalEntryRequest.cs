using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.JournalEntry
{
    public class DataTableJournalEntryRequest : BaseDatatableRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Status { get; set; }
        public Guid? AccountId { get; set; }
    }
}
