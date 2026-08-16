using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Return
{
    public class DataTableReturnRequest : BaseDatatableRequest
    {
        public string? ReturnType { get; set; }
        public string? Status { get; set; }
    }

    public class ReturnViewModel
    {
        public Guid GoodsReturnId { get; set; }
        public string GoodsReturnNumber { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReturnInvoiceOptionViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReturnInvoiceLineViewModel
    {
        public Guid InvoiceLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class ReturnInvoiceViewModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public List<ReturnInvoiceLineViewModel> Lines { get; set; } = new();
    }

    public class ReturnLineViewModel
    {
        public Guid GoodsReturnLineId { get; set; }
        public Guid InvoiceLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class ReturnLineRequest
    {
        public Guid InvoiceLineId { get; set; }
        public Guid? ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class CreateReturnRequest
    {
        public string ReturnType { get; set; } = "Sales";
        public DateTime ReturnDate { get; set; } = DateTime.Today;
        [Required]
        public Guid InvoiceId { get; set; }
        [Required]
        public Guid WarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<ReturnLineRequest> Lines { get; set; } = new();
    }

    public class UpdateReturnRequest : CreateReturnRequest
    {
        public Guid GoodsReturnId { get; set; }
    }

    public class ReturnDetailViewModel : ReturnViewModel
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? JournalNumber { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<ReturnLineViewModel> Lines { get; set; } = new();
    }
}