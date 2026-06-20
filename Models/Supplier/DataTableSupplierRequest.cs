using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.Supplier
{
    public class DataTableSupplierRequest : BaseDatatableRequest
    {
        public string? SupplierType { get; set; }
        public bool? IsActive { get; set; }
    }
}
