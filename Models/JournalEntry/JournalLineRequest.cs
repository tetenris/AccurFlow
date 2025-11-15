using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.JournalEntry
{
    public class JournalLineRequest
    {
        [Required]
        public Guid AccountId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DebitAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditAmount { get; set; }
    }
}
