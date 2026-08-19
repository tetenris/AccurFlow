using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class DeliveryOrderEntity : BaseEntity
    {
        public Guid DeliveryOrderId { get; set; }
        public string DeliveryNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public Guid SalesOrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal TotalAmount { get; set; }
        public Guid? InvoiceId { get; set; }
        public string? Notes { get; set; }
        public SalesOrderEntity SalesOrder { get; set; } = null!;
        public CustomerEntity Customer { get; set; } = null!;
        public InvoiceEntity? Invoice { get; set; }
        public ICollection<DeliveryOrderLineEntity> Lines { get; set; } = new List<DeliveryOrderLineEntity>();
    }
}
