using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class StockOpnameEntity : BaseEntity
    {
        public Guid StockOpnameId { get; set; }
        public string StockOpnameNumber { get; set; } = string.Empty;
        public DateTime OpnameDate { get; set; }
        public Guid WarehouseId { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Notes { get; set; }
        public WarehouseEntity Warehouse { get; set; } = null!;
        public ICollection<StockOpnameLineEntity> Lines { get; set; } = new List<StockOpnameLineEntity>();
    }
}

