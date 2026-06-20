using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IAccountService
    {
        Task<UserEntity?> ValidateUser(string username, string password);
    }

    public class AccountService : BaseService, IAccountService
    {
        public AccountService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<UserEntity?> ValidateUser(string username, string password)
        {
            var user = await _dbContext.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserName == username && !x.IsDeleted && x.IsActive);

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
    }
}
