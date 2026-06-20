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
