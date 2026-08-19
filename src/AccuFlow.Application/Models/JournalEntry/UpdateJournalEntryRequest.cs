using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.JournalEntry
{
    public class UpdateJournalEntryRequest
    {
        [Required]
        public Guid JournalId { get; set; }

        [Required]
        public DateTime JournalDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "At least 2 journal lines are required")]
        public List<JournalLineRequest> JournalLines { get; set; } = new();
    }
}
