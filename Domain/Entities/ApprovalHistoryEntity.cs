using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class ApprovalHistoryEntity : BaseEntity
    {
        public Guid ApprovalHistoryId { get; set; }
        public Guid ApprovalRequestId { get; set; }
        public Guid ApproverId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime ActionAt { get; set; }
        public string? Notes { get; set; }
        public ApprovalRequestEntity ApprovalRequest { get; set; } = null!;
        public UserEntity Approver { get; set; } = null!;
    }
}

