using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.JournalEntry
{
    public class CreateJournalEntryRequest
    {
        [Required]
        public DateTime JournalDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string JournalType { get; set; } = "General"; // General, Adjustment, Memo

        [Required]
        [MinLength(2, ErrorMessage = "At least 2 journal lines are required")]
        public List<JournalLineRequest> JournalLines { get; set; } = new();
    }
}
