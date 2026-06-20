using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AccuFlow.Services
{
    public interface IAccountService
    {
        Task<LoginResult> ValidateUser(string username, string password);
        Task<bool> RequestPasswordResetAsync(string email);
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }

    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public bool IsLocked { get; set; }
        public UserEntity? User { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AccountService : BaseService, IAccountService
    {
        public AccountService(AppDbContext dbContext) : base(dbContext)
        {
        }

        private const int MaxFailedLoginAttempts = 3;
        private const int PasswordExpiryDays = 30;

        public async Task<LoginResult> ValidateUser(string username, string password)
        {
            var normalizedUsername = username.Trim().ToLower();
            var user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => !x.IsDeleted
                    && x.IsActive
                    && (x.UserName.ToLower() == normalizedUsername || x.Email.ToLower() == normalizedUsername));

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Failed("Invalid username or password");
            }

            if (user.IsLocked)
            {
                return Failed("Password anda terkunci. Silakan hubungi admin.", true);
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValidPassword)
            {
                user.FailedLoginAttempts += 1;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.IsLocked = true;
                    user.LockedAt = DateTime.UtcNow;
                    user.LockedReason = "Password salah 3 kali berturut-turut";
                    await _dbContext.SaveChangesAsync();
                    return Failed("Password anda terkunci. Silakan hubungi admin.", true);
                }

                await _dbContext.SaveChangesAsync();
                var remainingAttempts = MaxFailedLoginAttempts - user.FailedLoginAttempts;
                return Failed($"Invalid username or password. Sisa percobaan: {remainingAttempts}");
            }

            if (user.FailedLoginAttempts > 0 || user.LockedAt != null || !string.IsNullOrWhiteSpace(user.LockedReason))
            {
                user.FailedLoginAttempts = 0;
                user.LockedAt = null;
                user.LockedReason = null;
                await _dbContext.SaveChangesAsync();
            }

            return new LoginResult
            {
                Succeeded = true,
                User = user
            };
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.IsActive && x.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                return false;
            }

            // Email delivery is not configured yet. This method is the integration point for future email reset links.
            return true;
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted && x.IsActive);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new Exception("User not found");
            }

            var isValidCurrentPassword = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
            if (!isValidCurrentPassword)
            {
                throw new Exception("Current password is incorrect");
            }

            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                throw new Exception("New password must be different from current password");
            }

            if (!Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$"))
            {
                throw new Exception("New password must contain uppercase letter, lowercase letter, number, and symbol");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.PasswordExpiresAt = DateTime.UtcNow.AddDays(PasswordExpiryDays);
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedAt = null;
            user.LockedReason = null;
            user.UpdatedBy = userId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private static LoginResult Failed(string message, bool isLocked = false)
        {
            return new LoginResult
            {
                Succeeded = false,
                IsLocked = isLocked,
                Message = message
            };
        }
    }
}
