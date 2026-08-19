using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class ApprovalRequestEntity : BaseEntity
    {
        public Guid ApprovalRequestId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string Status { get; set; } = "Pending";
        public Guid RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public Guid? CurrentApproverId { get; set; }
        public string? Notes { get; set; }
        public UserEntity RequestedByUser { get; set; } = null!;
        public UserEntity? CurrentApprover { get; set; }
        public ICollection<ApprovalHistoryEntity> Histories { get; set; } = new List<ApprovalHistoryEntity>();
    }
}

