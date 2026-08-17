using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.StockTransfer
{
    public class DataTableStockTransferRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
        public Guid? WarehouseId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class StockTransferViewModel
    {
        public Guid StockTransferId { get; set; }
        public string StockTransferNumber { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public string FromWarehouseName { get; set; } = string.Empty;
        public string ToWarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class CreateStockTransferRequest
    {
        public DateTime TransferDate { get; set; } = DateTime.Today;
        public Guid FromWarehouseId { get; set; }
        public Guid ToWarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<CreateStockTransferLineRequest> Lines { get; set; } = new();
    }

    public class UpdateStockTransferRequest : CreateStockTransferRequest
    {
        public Guid StockTransferId { get; set; }
    }

    public class StockTransferDetailViewModel : StockTransferViewModel
    {
        public Guid FromWarehouseId { get; set; }
        public Guid ToWarehouseId { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<StockTransferLineViewModel> Lines { get; set; } = new();
    }

    public class StockTransferLineViewModel
    {
        public Guid StockTransferLineId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class CreateStockTransferLineRequest
    {
        [Required]
        public Guid ItemId { get; set; }
        public decimal Quantity { get; set; } = 1;
    }
}