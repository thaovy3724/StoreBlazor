using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
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
