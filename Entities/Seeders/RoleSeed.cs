using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class RoleSeed
    {
        /// <summary>
        /// Returns predefined role seed data for the application
        /// </summary>
        public static List<RoleEntity> GetRoleSeedData()
        {
            return new List<RoleEntity>
            {
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    RoleType = Enums.RoleEnum.Administrator,
                    RoleName = "Administrator",
                    Description = "Full system access",
                    Permissions = "[\"all\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    RoleType = Enums.RoleEnum.Accountant,
                    RoleName = "Accountant",
                    Description = "Accounting operations access",
                    Permissions = "[\"journal_entry\",\"invoice\",\"report\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    RoleType = Enums.RoleEnum.Manager,
                    RoleName = "Manager",
                    Description = "Management and approval access",
                    Permissions = "[\"view\",\"approve\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                },
                new RoleEntity
                {
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    RoleType = Enums.RoleEnum.User,
                    RoleName = "User",
                    Description = "Basic user access",
                    Permissions = "[\"view\"]",
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
