using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Delivery
{
    public class DataTableDeliveryRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class DeliveryViewModel
    {
        public Guid DeliveryOrderId { get; set; }
        public string DeliveryNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderOptionViewModel
    {
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }

    public class OrderDeliveryLineViewModel
    {
        public Guid SalesOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderDeliveryViewModel
    {
        public Guid SalesOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderDeliveryLineViewModel> Lines { get; set; } = new();
    }

    public class DeliveryLineViewModel
    {
        public Guid DeliveryOrderLineId { get; set; }
        public Guid SalesOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class DeliveryLineRequest
    {
        public Guid SalesOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateDeliveryRequest
    {
        public DateTime DeliveryDate { get; set; } = DateTime.Today;
        [Required]
        public Guid SalesOrderId { get; set; }
        public string? Notes { get; set; }
        public List<DeliveryLineRequest> Lines { get; set; } = new();
    }

    public class UpdateDeliveryRequest : CreateDeliveryRequest
    {
        public Guid DeliveryOrderId { get; set; }
    }

    public class DeliveryDetailViewModel : DeliveryViewModel
    {
        public string? Notes { get; set; }
        public string? InvoiceNumber { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public bool CanInvoice { get; set; }
        public List<DeliveryLineViewModel> Lines { get; set; } = new();
    }
}