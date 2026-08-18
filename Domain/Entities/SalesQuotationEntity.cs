using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class SalesQuotationEntity : BaseEntity
    {
        public Guid SalesQuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public CustomerEntity Customer { get; set; } = null!;
        public ICollection<SalesQuotationLineEntity> Lines { get; set; } = new List<SalesQuotationLineEntity>();
    }
}
