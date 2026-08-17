using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class BillOfMaterialEntity : BaseEntity
    {
        public Guid BomId { get; set; }
        public string BomNumber { get; set; } = string.Empty;
        public Guid FinishedItemId { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public ItemEntity FinishedItem { get; set; } = null!;
        public virtual ICollection<BillOfMaterialLineEntity> Lines { get; set; } = new List<BillOfMaterialLineEntity>();
    }

    public class BillOfMaterialLineEntity : BaseEntity
    {
        public Guid BomLineId { get; set; }
        public Guid BomId { get; set; }
        public Guid ComponentItemId { get; set; }
        public decimal QuantityPerUnit { get; set; }
        public BillOfMaterialEntity Bom { get; set; } = null!;
        public ItemEntity ComponentItem { get; set; } = null!;
    }

    public class ProductionOrderEntity : BaseEntity
    {
        public Guid ProductionOrderId { get; set; }
        public string ProductionOrderNumber { get; set; } = string.Empty;
        public Guid? BomId { get; set; }
        public Guid FinishedItemId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public Guid WarehouseId { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Posted
        public Guid? JournalId { get; set; }
        public string? Notes { get; set; }
        public BillOfMaterialEntity? Bom { get; set; }
        public ItemEntity FinishedItem { get; set; } = null!;
        public WarehouseEntity Warehouse { get; set; } = null!;
        public JournalEntryEntity? JournalEntry { get; set; }
        public virtual ICollection<ProductionOrderLineEntity> Lines { get; set; } = new List<ProductionOrderLineEntity>();
    }

    public class ProductionOrderLineEntity : BaseEntity
    {
        public Guid ProductionOrderLineId { get; set; }
        public Guid ProductionOrderId { get; set; }
        public Guid ComponentItemId { get; set; }
        public decimal QuantityRequired { get; set; }
        public decimal UnitCost { get; set; }
        public ProductionOrderEntity ProductionOrder { get; set; } = null!;
        public ItemEntity ComponentItem { get; set; } = null!;
    }
}