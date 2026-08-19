using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.JournalEntry
{
    public class ReverseJournalRequest
    {
        [Required]
        public Guid JournalId { get; set; }

        [Required]
        public DateTime ReversalDate { get; set; }
    }
}
