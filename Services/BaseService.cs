using AccuFlow.Entities.Context;

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
    }
}
