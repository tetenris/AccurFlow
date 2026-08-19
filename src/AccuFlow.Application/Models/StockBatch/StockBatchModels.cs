namespace AccuFlow.Models.StockBatch
{
    public class StockBatchCreateRequest
    {
        public Guid ItemId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class StockBatchConsumeRequest
    {
        public Guid StockBatchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class StockBatchViewModel
    {
        public Guid StockBatchId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ConsumedQuantity => Quantity - RemainingQuantity;
    }
}