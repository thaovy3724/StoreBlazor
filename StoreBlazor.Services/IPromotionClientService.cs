using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public interface IPromotionClientService
    {
        Task<List<Promotion>> GetAllPromotionAsync();
    }
}
