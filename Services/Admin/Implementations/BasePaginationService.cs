using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using System.Linq.Expressions;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class BasePaginationService : BaseService
    {
        private int PageSize = 5;

        public BasePaginationService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PageResult<T>> GetPagedAsync<T>(IQueryable<T> query, int page)
            where T : class
        {

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new PageResult<T>
            {
                Items = items,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)

            };
        }
    }
}
