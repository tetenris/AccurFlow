using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class SalesOrderLineEntity : BaseEntity
    {
        public Guid SalesOrderLineId { get; set; }
        public Guid SalesOrderId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public SalesOrderEntity SalesOrder { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}
