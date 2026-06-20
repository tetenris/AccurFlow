using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Models.User
{
    public class UserViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? PasswordExpiresAt { get; set; }
        public DateTime? LockedAt { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public Guid RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}
