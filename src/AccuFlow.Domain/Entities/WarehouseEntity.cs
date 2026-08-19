using AccuFlow.Domain.Common;

namespace AccuFlow.Domain.Entities
{
    public class WarehouseEntity : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

