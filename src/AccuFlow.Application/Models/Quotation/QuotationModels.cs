using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Quotation
{
    public class DataTableQuotationRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class QuotationViewModel
    {
        public Guid SalesQuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class QuotationLineViewModel
    {
        public Guid SalesQuotationLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class QuotationLineRequest
    {
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class CreateQuotationRequest
    {
        public DateTime QuotationDate { get; set; } = DateTime.Today;
        public DateTime? ValidUntil { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        public string? Notes { get; set; }
        public List<QuotationLineRequest> Lines { get; set; } = new();
    }

    public class UpdateQuotationRequest : CreateQuotationRequest
    {
        public Guid SalesQuotationId { get; set; }
    }

    public class QuotationDetailViewModel : QuotationViewModel
    {
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public List<QuotationLineViewModel> Lines { get; set; } = new();
    }
}