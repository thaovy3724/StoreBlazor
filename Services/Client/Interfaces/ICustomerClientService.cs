using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface ICustomerClientService
    {
        Task<Customer?> GetByEmailAsync(string email);
    }
}
