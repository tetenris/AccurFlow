using System.ComponentModel;

namespace AccuFlow.Entities.Enums
{
    public enum RoleEnum
    {
        [Description("Super Administrator")]
        SuperAdministrator = 1,

        [Description("Administrator")]
        Administrator = 2,

        [Description("Manager")]
        Manager = 3,

        [Description("Accountant")]
        Accountant = 4,

        [Description("Finance Staff")]
        FinanceStaff = 5,

        [Description("AR Officer")]
        AROfficer = 6,

        [Description("AP Officer")]
        APOfficer = 7,

        [Description("Purchasing")]
        Purchasing = 8,

        [Description("Sales")]
        Sales = 9,

        [Description("Warehouse")]
        Warehouse = 10,

        [Description("Viewer")]
        Viewer = 11
    }
}
