namespace AccuFlow.Models.Production
{
    public class BomRequest
    {
        public Guid FinishedItemId { get; set; }
        public string? Notes { get; set; }
        public List<BomLineRequest> Lines { get; set; } = new();
    }

    public class BomLineRequest
    {
        public Guid ComponentItemId { get; set; }
        public decimal QuantityPerUnit { get; set; }
    }

    public class BomViewModel
    {
        public Guid BomId { get; set; }
        public string BomNumber { get; set; } = string.Empty;
        public Guid FinishedItemId { get; set; }
        public string FinishedItemCode { get; set; } = string.Empty;
        public string FinishedItemName { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class BomDetailViewModel
    {
        public Guid BomId { get; set; }
        public string BomNumber { get; set; } = string.Empty;
        public Guid FinishedItemId { get; set; }
        public string FinishedItemCode { get; set; } = string.Empty;
        public string FinishedItemName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public List<BomLineViewModel> Lines { get; set; } = new();
    }

    public class BomLineViewModel
    {
        public Guid BomLineId { get; set; }
        public Guid ComponentItemId { get; set; }
        public string ComponentItemCode { get; set; } = string.Empty;
        public string ComponentItemName { get; set; } = string.Empty;
        public decimal QuantityPerUnit { get; set; }
    }

    public class CreateProductionOrderRequest
    {
        public Guid? BomId { get; set; }
        public Guid FinishedItemId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public Guid WarehouseId { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductionOrderViewModel
    {
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string? BomNumber { get; set; }
        public string FinishedItemCode { get; set; } = string.Empty;
        public string FinishedItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string? JournalNumber { get; set; }
        public string? Notes { get; set; }
        public int LineCount { get; set; }
    }

    public class ProductionOrderDetailViewModel
    {
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public string? BomNumber { get; set; }
        public string FinishedItemCode { get; set; } = string.Empty;
        public string FinishedItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public string? JournalNumber { get; set; }
        public bool CanPost => Status == "Draft";
        public List<ProductionOrderLineViewModel> Lines { get; set; } = new();
    }

    public class ProductionOrderLineViewModel
    {
        public Guid ProductionOrderLineId { get; set; }
        public Guid ComponentItemId { get; set; }
        public string ComponentItemCode { get; set; } = string.Empty;
        public string ComponentItemName { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineCost => QuantityRequired * UnitCost;
    }
}