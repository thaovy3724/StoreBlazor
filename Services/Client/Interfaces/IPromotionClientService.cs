using StoreBlazor.Models;

namespace StoreBlazor.Services.Client.Interfaces
{
    public interface IPromotionClientService
    {
        Task<List<Promotion>> GetAllPromotionAsync();
    }
}
