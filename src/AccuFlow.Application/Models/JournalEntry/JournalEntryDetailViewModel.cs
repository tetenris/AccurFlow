namespace AccuFlow.Models.JournalEntry
{
    public class JournalEntryDetailViewModel
    {
        public Guid JournalId { get; set; }
        public string JournalNumber { get; set; } = string.Empty;
        public DateTime JournalDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string JournalType { get; set; } = "General";
        public string Status { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced { get; set; }
        public DateTime? PostedDate { get; set; }
        public string? PostedBy { get; set; }
        public string? ReversalJournalNumber { get; set; }
        public string? OriginalJournalNumber { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public bool CanReverse { get; set; }
        public List<JournalLineViewModel> JournalLines { get; set; } = new();
    }
}
