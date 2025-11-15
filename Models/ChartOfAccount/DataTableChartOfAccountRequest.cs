using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.ChartOfAccount
{
    public class DataTableChartOfAccountRequest : BaseDatatableRequest
    {
        public string? AccountType { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsHeader { get; set; }
        public Guid? ParentAccountId { get; set; }
    }
}
