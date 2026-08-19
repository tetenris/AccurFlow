using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class StockBatchEntity : BaseEntity
    {
        public Guid StockBatchId { get; set; }
        public Guid ItemId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public ItemEntity Item { get; set; } = null!;
    }
}
