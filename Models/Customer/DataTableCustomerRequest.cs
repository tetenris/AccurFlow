using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.Customer
{
    public class DataTableCustomerRequest : BaseDatatableRequest
    {
        public string? CustomerType { get; set; }
        public bool? IsActive { get; set; }
    }
}
