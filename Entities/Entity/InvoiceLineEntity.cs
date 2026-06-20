using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class InvoiceLineEntity : BaseEntity
    {
        public Guid InvoiceLineId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? AccountId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public InvoiceEntity Invoice { get; set; } = null!;
        public ItemEntity? Item { get; set; }
        public ChartOfAccountEntity? Account { get; set; }
    }
}
