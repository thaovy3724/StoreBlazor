using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;

namespace StoreBlazor.Services.Client.Implementations
{
    public class PromotionClientService : BaseService, IPromotionClientService
    {
        public PromotionClientService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
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
