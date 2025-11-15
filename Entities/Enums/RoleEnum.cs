using System.ComponentModel;

namespace AccuFlow.Entities.Enums
{
    public enum RoleEnum
    {
        [Description("Administrator")]
        Administrator = 1,

        [Description("Accountant")]
        Accountant = 2,

        [Description("Manager")]
        Manager = 3,

        [Description("User")]
        User = 4
    }
}
