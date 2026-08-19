using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.Approval
{
    public class DataTableApprovalRequest : BaseDatatableRequest
    {
        public string? DocumentType { get; set; }
        public string? Status { get; set; }
    }

    public class ApprovalRequestViewModel
    {
        public Guid ApprovalRequestId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class SubmitApprovalRequest
    {
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public Guid? CurrentApproverId { get; set; }
        public string? Notes { get; set; }
    }

    public class ApprovalActionRequest
    {
        public Guid ApprovalRequestId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
