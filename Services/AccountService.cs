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
            // TODO: Implement proper password hashing
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.UserName == username && !x.IsDeleted && x.IsActive);

            return user;
        }
    }
}
