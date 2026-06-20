using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.PurchaseOrder
{
    public class DataTablePurchaseOrderRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PurchaseOrderViewModel
    {
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class CreatePurchaseOrderRequest
    {
        public DateTime OrderDate { get; set; } = DateTime.Today;
        public DateTime? ExpectedDate { get; set; }
        public Guid SupplierId { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderLineRequest> Lines { get; set; } = new();
    }

    public class UpdatePurchaseOrderRequest : CreatePurchaseOrderRequest
    {
        public Guid PurchaseOrderId { get; set; }
    }

    public class PurchaseOrderDetailViewModel : PurchaseOrderViewModel
    {
        public Guid SupplierId { get; set; }
        public Guid? PurchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNumber { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanConvert { get; set; }
        public List<PurchaseOrderLineViewModel> Lines { get; set; } = new();
    }

    public class PurchaseOrderLineViewModel
    {
        public Guid PurchaseOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string? ItemName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreatePurchaseOrderLineRequest
    {
        public Guid? ItemId { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
