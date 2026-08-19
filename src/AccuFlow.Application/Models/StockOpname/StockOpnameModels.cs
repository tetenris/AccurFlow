using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.StockOpname
{
    public class DataTableStockOpnameRequest : BaseDatatableRequest
    {
        public string? Status { get; set; }
    }

    public class StockOpnameViewModel
    {
        public Guid StockOpnameId { get; set; }
        public string StockOpnameNumber { get; set; } = string.Empty;
        public DateTime OpnameDate { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalDifference { get; set; }
    }

    public class WarehouseViewModel
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
    }

    public class StockOpnameLineViewModel
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal SystemQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal DifferenceQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    public class StockOpnameLineRequest
    {
        public Guid ItemId { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal DifferenceQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateStockOpnameRequest
    {
        public DateTime OpnameDate { get; set; } = DateTime.Today;
        [Required]
        public Guid WarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<StockOpnameLineRequest> Lines { get; set; } = new();
    }

    public class UpdateStockOpnameRequest : CreateStockOpnameRequest
    {
        public Guid StockOpnameId { get; set; }
    }

    public class StockOpnameDetailViewModel : StockOpnameViewModel
    {
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPost { get; set; }
        public List<StockOpnameLineViewModel> Lines { get; set; } = new();
    }
}