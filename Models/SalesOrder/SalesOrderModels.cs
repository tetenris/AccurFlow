using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.SalesOrder
{
    public class DataTableOrderRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class OrderViewModel
    {
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string QuotationNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderLineViewModel
    {
        public Guid SalesOrderLineId { get; set; }
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

    public class OrderLineRequest
    {
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class CreateOrderRequest
    {
        public DateTime OrderDate { get; set; } = DateTime.Today;
        public DateTime? ExpectedDate { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        public Guid? QuotationId { get; set; }
        public string? Notes { get; set; }
        public List<OrderLineRequest> Lines { get; set; } = new();
    }

    public class UpdateOrderRequest : CreateOrderRequest
    {
        public Guid SalesOrderId { get; set; }
    }

    public class OrderDetailViewModel : OrderViewModel
    {
        public Guid CustomerId { get; set; }
        public Guid? QuotationId { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public List<OrderLineViewModel> Lines { get; set; } = new();
    }

    public class QuoteOptionViewModel
    {
        public Guid SalesQuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
    }
}