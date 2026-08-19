using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class StockTransferEntity : BaseEntity
    {
        public Guid StockTransferId { get; set; }
        public string StockTransferNumber { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public Guid FromWarehouseId { get; set; }
        public Guid ToWarehouseId { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Notes { get; set; }
        public WarehouseEntity FromWarehouse { get; set; } = null!;
        public WarehouseEntity ToWarehouse { get; set; } = null!;
        public ICollection<StockTransferLineEntity> Lines { get; set; } = new List<StockTransferLineEntity>();
    }

    public class StockTransferLineEntity : BaseEntity
    {
        public Guid StockTransferLineId { get; set; }
        public Guid StockTransferId { get; set; }
        public Guid ItemId { get; set; }
        public decimal Quantity { get; set; }
        public StockTransferEntity StockTransfer { get; set; } = null!;
        public ItemEntity Item { get; set; } = null!;
    }
}
