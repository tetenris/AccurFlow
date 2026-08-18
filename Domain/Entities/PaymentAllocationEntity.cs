using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class PaymentAllocationEntity : BaseEntity
    {
        public Guid PaymentAllocationId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public PaymentEntity Payment { get; set; } = null!;
        public InvoiceEntity Invoice { get; set; } = null!;
    }
}

