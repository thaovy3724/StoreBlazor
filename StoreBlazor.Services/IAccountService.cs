using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services;

public interface IAccountService
{
    Task<PageResult<User>> GetAllAccountAsync(int page);
    Task<ServiceResult> CreateAsync(User model);
    Task<ServiceResult> UpdateAsync(User model);
    Task<ServiceResult> DeleteAsync(User model);
    Task<PageResult<User>> SearchByNameAsync(string keyword, int page);
}