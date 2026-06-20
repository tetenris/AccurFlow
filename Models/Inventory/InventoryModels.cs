using AccuFlow.Models.BaseModel;
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.Inventory
{
    public class DataTableItemRequest : BaseDatatableRequest
    {
        public string? ItemType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ItemViewModel
    {
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateItemRequest
    {
        [Required]
        public string ItemCode { get; set; } = string.Empty;
        [Required]
        public string ItemName { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Inventory";
        public string Unit { get; set; } = "PCS";
        public decimal SalesPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? InventoryAccountId { get; set; }
        public Guid? SalesAccountId { get; set; }
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public string? Description { get; set; }
    }

    public class DataTableStockMovementRequest : BaseDatatableRequest
    {
        public Guid? ItemId { get; set; }
        public Guid? WarehouseId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class StockMovementViewModel
    {
        public Guid StockMovementId { get; set; }
        public DateTime MovementDate { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
        public decimal UnitCost { get; set; }
    }
}
