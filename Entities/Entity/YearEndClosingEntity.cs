using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class YearEndClosingEntity : BaseEntity
    {
        public Guid ClosingId { get; set; }
        public int FiscalYear { get; set; }
        public DateTime ClosingDate { get; set; }
        public Guid? ClosingJournalId { get; set; }
        public JournalEntryEntity? ClosingJournal { get; set; }
    }
}