using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccuFlow.Entities.Entity
{
    [Table("JournalLines")]
    public class JournalLineEntity
    {
        [Key]
        public Guid JournalLineId { get; set; }

        [Required]
        public Guid JournalId { get; set; }

        [Required]
        public int LineNumber { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }

        // Audit fields
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        // Navigation properties
        public virtual JournalEntryEntity JournalEntry { get; set; } = null!;
        public virtual ChartOfAccountEntity Account { get; set; } = null!;

        // Computed property
        [NotMapped]
        public decimal Amount => DebitAmount > 0 ? DebitAmount : CreditAmount;
    }
}
