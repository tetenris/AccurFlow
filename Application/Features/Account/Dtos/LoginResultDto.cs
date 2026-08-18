namespace AccuFlow.Application.Features.Account.Dtos
{
    public class LoginResultDto
    {
        public bool Succeeded { get; set; }
        public bool IsLocked { get; set; }
        public string Message { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime? PasswordExpiresAt { get; set; }
    }
}