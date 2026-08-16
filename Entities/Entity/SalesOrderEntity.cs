using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class SalesOrderEntity : BaseEntity
    {
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? QuotationId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public CustomerEntity Customer { get; set; } = null!;
        public SalesQuotationEntity? Quotation { get; set; }
        public ICollection<SalesOrderLineEntity> Lines { get; set; } = new List<SalesOrderLineEntity>();
    }
}