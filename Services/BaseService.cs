using AccuFlow.Entities.Context;
using AccuFlow.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IBaseService
    {
    }

    public class BaseService : IBaseService
    {
        protected readonly AppDbContext _dbContext;

        public BaseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get user name (FullName or UserName) from user ID string for display
        /// Returns "System" if user not found or invalid GUID
        /// </summary>
        protected async Task<string> GetUserNameFromIdAsync(string userIdString)
        {
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return "System";
            }

            var user = await _dbContext.Set<UserEntity>()
                .Where(u => u.UserId == userId)
                .Select(u => u.FullName ?? u.UserName)
                .FirstOrDefaultAsync();
            
            return user ?? "System";
        }
    }
}
