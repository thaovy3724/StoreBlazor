using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class CustomerClientService : BaseService, ICustomerClientService
    {
        public CustomerClientService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbContext.Customers
                .FirstOrDefaultAsync(x => x.Email == email);
        }

    }
}
