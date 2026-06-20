using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
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
