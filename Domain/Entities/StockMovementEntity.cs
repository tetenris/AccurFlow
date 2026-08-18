using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class StockMovementEntity : BaseEntity
    {
        public Guid StockMovementId { get; set; }
        public DateTime MovementDate { get; set; }
        public Guid ItemId { get; set; }
        public Guid WarehouseId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string SourceDocumentType { get; set; } = string.Empty;
        public Guid? SourceDocumentId { get; set; }
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
        public decimal UnitCost { get; set; }
        public string? Notes { get; set; }
        public ItemEntity Item { get; set; } = null!;
        public WarehouseEntity Warehouse { get; set; } = null!;
    }
}

