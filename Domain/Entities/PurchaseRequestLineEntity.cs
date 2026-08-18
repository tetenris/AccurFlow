using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class PurchaseRequestLineEntity : BaseEntity
    {
        public Guid PurchaseRequestLineId { get; set; }
        public Guid PurchaseRequestId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public PurchaseRequestEntity PurchaseRequest { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}
