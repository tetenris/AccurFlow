using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccuFlow.Entities.Entity
{
    [Table("JournalEntries")]
    public class JournalEntryEntity
    {
        [Key]
        public Guid JournalId { get; set; }

        [Required]
        [MaxLength(50)]
        public string JournalNumber { get; set; } = string.Empty;

        [Required]
        public DateTime JournalDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Posted, Reversed

        [Required]
        [MaxLength(30)]
        public string JournalType { get; set; } = "General"; // General, Adjustment, Memo

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; }

        public DateTime? PostedDate { get; set; }

        public Guid? PostedBy { get; set; }

        public Guid? ReversalJournalId { get; set; }

        public Guid? OriginalJournalId { get; set; }

        // Audit fields
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<JournalLineEntity> JournalLines { get; set; } = new List<JournalLineEntity>();
        public virtual UserEntity? CreatedByUser { get; set; }
        public virtual UserEntity? UpdatedByUser { get; set; }
        public virtual UserEntity? PostedByUser { get; set; }
        public virtual UserEntity? DeletedByUser { get; set; }
        public virtual JournalEntryEntity? ReversalJournal { get; set; }
        public virtual JournalEntryEntity? OriginalJournal { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsBalanced => TotalDebit == TotalCredit;

        [NotMapped]
        public bool CanEdit => Status == "Draft" && !IsDeleted;

        [NotMapped]
        public bool CanDelete => Status == "Draft" && !IsDeleted;

        [NotMapped]
        public bool CanPost => Status == "Draft" && !IsDeleted && IsBalanced;

        [NotMapped]
        public bool CanReverse => Status == "Posted" && !IsDeleted;
    }
}
