using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class DeliveryOrderLineEntity : BaseEntity
    {
        public Guid DeliveryOrderLineId { get; set; }
        public Guid DeliveryOrderId { get; set; }
        public Guid SalesOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public DeliveryOrderEntity DeliveryOrder { get; set; } = null!;
        public SalesOrderLineEntity SalesOrderLine { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}
