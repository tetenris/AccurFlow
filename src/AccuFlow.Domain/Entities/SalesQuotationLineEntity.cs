using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class SalesQuotationLineEntity : BaseEntity
    {
        public Guid SalesQuotationLineId { get; set; }
        public Guid SalesQuotationId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public SalesQuotationEntity SalesQuotation { get; set; } = null!;
        public ItemEntity? Item { get; set; }
    }
}
