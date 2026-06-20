using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AccuFlow.Services
{
    public interface IAccountService
    {
        Task<UserEntity?> ValidateUser(string username, string password);
        Task<bool> RequestPasswordResetAsync(string email);
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }

    public class AccountService : BaseService, IAccountService
    {
        public AccountService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserEntity?> ValidateUser(string username, string password)
        {
            var normalizedUsername = username.Trim().ToLower();
            var user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => !x.IsDeleted
                    && x.IsActive
                    && (x.UserName.ToLower() == normalizedUsername || x.Email.ToLower() == normalizedUsername));

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return null;
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValidPassword)
            {
                return null;
            }

            return user;
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
            user.UpdatedBy = userId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }
}
