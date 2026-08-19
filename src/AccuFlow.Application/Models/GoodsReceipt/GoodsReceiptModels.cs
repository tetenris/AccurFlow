using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.GoodsReceipt
{
    public class DataTableGoodsReceiptRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class GoodsReceiptViewModel
    {
        public Guid GoodsReceiptId { get; set; }
        public string GoodsReceiptNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PurchaseOrderOptionViewModel
    {
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PurchaseOrderReceiptLineViewModel
    {
        public Guid PurchaseOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class PurchaseOrderReceiptViewModel
    {
        public Guid PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<PurchaseOrderReceiptLineViewModel> Lines { get; set; } = new();
    }

    public class GoodsReceiptLineViewModel
    {
        public Guid GoodsReceiptLineId { get; set; }
        public Guid PurchaseOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class GoodsReceiptLineRequest
    {
        public Guid PurchaseOrderLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class CreateGoodsReceiptRequest
    {
        public DateTime ReceiptDate { get; set; } = DateTime.Today;
        [Required]
        public Guid PurchaseOrderId { get; set; }
        [Required]
        public Guid WarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<GoodsReceiptLineRequest> Lines { get; set; } = new();
    }

    public class UpdateGoodsReceiptRequest : CreateGoodsReceiptRequest
    {
        public Guid GoodsReceiptId { get; set; }
    }

    public class GoodsReceiptDetailViewModel : GoodsReceiptViewModel
    {
        public Guid PurchaseOrderId { get; set; }
        public string? Notes { get; set; }
        public string? JournalNumber { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<GoodsReceiptLineViewModel> Lines { get; set; } = new();
    }
}