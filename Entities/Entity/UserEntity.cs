using AccuFlow.Entities.Abstractions;

namespace AccuFlow.Entities.Entity
{
    public class UserEntity : BaseEntity, IEntity
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? PasswordChangedAt { get; set; }
        public DateTime? PasswordExpiresAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedReason { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Role relationship
        public Guid RoleId { get; set; }
        public RoleEntity? Role { get; set; }
    }
}
