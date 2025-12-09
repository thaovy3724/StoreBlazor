using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface ICustomerClientService
    {
        Task<Customer?> GetByEmailAsync(string email);
    }
}
