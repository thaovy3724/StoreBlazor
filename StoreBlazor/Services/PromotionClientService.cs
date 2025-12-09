using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class PromotionClientService(ApplicationDbContext dbContext)
        : BaseService(dbContext), IPromotionClientService
    {
        public async Task<List<Promotion>> GetAllPromotionAsync()
        {
            var list = await _dbContext.Promotions
                        .Where(p => p.Status == PromotionStatus.Active
                            && p.EndDate >= DateTime.Now
                            && p.UsageLimit > p.UsedCount)
                        .ToListAsync();

            return list;
        }
    }
}
