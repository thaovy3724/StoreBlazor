using StoreBlazor.Data;

namespace StoreBlazor.Services
{
    public class BaseService
    {
        protected readonly ApplicationDbContext _dbContext;
        public BaseService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
