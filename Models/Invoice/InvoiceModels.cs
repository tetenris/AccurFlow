using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Invoice
{
    public class DataTableInvoiceRequest : BaseDatatableRequest
    {
        public string? InvoiceType { get; set; }
        public string? Status { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class InvoiceViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount => TotalAmount - PaidAmount;
    }

    public class CreateInvoiceRequest
    {
        [Required]
        public string InvoiceType { get; set; } = "Sales";
        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Notes { get; set; }
        public List<CreateInvoiceLineRequest> Lines { get; set; } = new();
    }

    public class CreateInvoiceLineRequest
    {
        public Guid? ItemId { get; set; }
        public Guid? AccountId { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
