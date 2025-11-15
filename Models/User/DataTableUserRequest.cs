using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.User
{
    public class DataTableUserRequest : BaseDatatableRequest
    {
        public bool? IsActive { get; set; }
        public Guid? RoleId { get; set; }
    }
}
