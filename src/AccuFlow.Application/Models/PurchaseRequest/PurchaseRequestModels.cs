using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.PurchaseRequest
{
    public class DataTablePurchaseRequestRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PurchaseRequestViewModel
    {
        public Guid PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PurchaseOrderNumber { get; set; }
    }

    public class CreatePurchaseRequestRequest
    {
        public DateTime RequestDate { get; set; } = DateTime.Today;
        public DateTime? RequiredDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseRequestLineRequest> Lines { get; set; } = new();
    }

    public class UpdatePurchaseRequestRequest : CreatePurchaseRequestRequest
    {
        public Guid PurchaseRequestId { get; set; }
    }

    public class ConvertPurchaseRequestRequest
    {
        public Guid PurchaseRequestId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime? ExpectedDate { get; set; }
    }

    public class PurchaseRequestDetailViewModel : PurchaseRequestViewModel
    {
        public string? Notes { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanConvert { get; set; }
        public List<PurchaseRequestLineViewModel> Lines { get; set; } = new();
    }

    public class PurchaseRequestLineViewModel
    {
        public Guid PurchaseRequestLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreatePurchaseRequestLineRequest
    {
        public Guid? ItemId { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }
}