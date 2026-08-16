using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class GoodsReturnLineEntity : BaseEntity
    {
        public Guid GoodsReturnLineId { get; set; }
        public Guid GoodsReturnId { get; set; }
        public Guid InvoiceLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public GoodsReturnEntity GoodsReturn { get; set; } = null!;
        public InvoiceLineEntity InvoiceLine { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}