using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.JournalEntry
{
    public class PostJournalRequest
    {
        [Required]
        public Guid JournalId { get; set; }

        [Required]
        public DateTime PostedDate { get; set; }
    }
}
