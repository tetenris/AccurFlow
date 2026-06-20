using AccuFlow.Entities.Entity;

namespace AccuFlow.Entities.Seeders
{
    public static class UserSeed
    {
        /// <summary>
        /// Returns predefined user seed data for the application
        /// </summary>
        public static List<UserEntity> GetUserSeedData()
        {
            return new List<UserEntity>
            {
                new UserEntity
                {
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    UserName = "admin",
                    Email = "admin@accuflow.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FullName = "System Administrator",
                    IsActive = true,
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // Super Administrator role
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new UserEntity
                {
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    UserName = "administrator",
                    Email = "administrator@accuflow.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FullName = "Default Administrator",
                    IsActive = true,
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"), // Administrator role
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new UserEntity
                {
                    UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    UserName = "accountant",
                    Email = "accountant@accuflow.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Accountant123!"),
                    FullName = "Default Accountant",
                    IsActive = true,
                    RoleId = Guid.Parse("00000000-0000-0000-0000-000000000004"), // Accountant role
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
