using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class PurchaseRequestEntity : BaseEntity
    {
        public Guid PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? Notes { get; set; }
        public PurchaseOrderEntity? PurchaseOrder { get; set; }
        public ICollection<PurchaseRequestLineEntity> Lines { get; set; } = new List<PurchaseRequestLineEntity>();
    }
}
