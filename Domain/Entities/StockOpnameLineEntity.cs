using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class StockOpnameLineEntity : BaseEntity
    {
        public Guid StockOpnameLineId { get; set; }
        public Guid StockOpnameId { get; set; }
        public Guid ItemId { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal DifferenceQuantity { get; set; }
        public string? Notes { get; set; }
        public StockOpnameEntity StockOpname { get; set; } = null!;
        public ItemEntity Item { get; set; } = null!;
    }
}

