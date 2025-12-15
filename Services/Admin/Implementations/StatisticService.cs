using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin.Statistic;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class StatisticService : IStatisticService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public StatisticService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<WeeklyStatisticDTO> GetWeeklyStatisticsAsync()
        {
            await using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            DateTime today = DateTime.Now;

            // Xác định tuần hiện tại
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-diff).Date;
            DateTime endOfWeek = startOfWeek.AddDays(7);

            // Tuần trước
            DateTime startOfLastWeek = startOfWeek.AddDays(-7);
            DateTime endOfLastWeek = startOfWeek;

            // Doanh thu tuần này
            var revenueThisWeek = await _dbContext.Orders
                .Where(o => o.OrderDate >= startOfWeek && o.OrderDate < endOfWeek && o.Status == OrderStatus.Paid)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Doanh thu tuần trước
            var revenueLastWeek = await _dbContext.Orders
                .Where(o => o.OrderDate >= startOfLastWeek && o.OrderDate < endOfLastWeek && o.Status == OrderStatus.Paid)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Đơn hàng tuần này
            var orderCount = await _dbContext.Orders
                .CountAsync(o => o.OrderDate >= startOfWeek && o.OrderDate < endOfWeek && o.Status == OrderStatus.Paid);

            // Đơn hàng tuần trước
            var orderCountLastWeek = await _dbContext.Orders
                .CountAsync(o => o.OrderDate >= startOfLastWeek && o.OrderDate < endOfLastWeek && o.Status == OrderStatus.Paid);

            // Khách hàng mới tuần này
            var newCustomers = await _dbContext.Customers
                .CountAsync(c => c.CreatedAt >= startOfWeek && c.CreatedAt < endOfWeek);

            // Khách hàng mới tuần trước
            var newCustomersLastWeek = await _dbContext.Customers
                .CountAsync(c => c.CreatedAt >= startOfLastWeek && c.CreatedAt < endOfLastWeek);

            // Sản phẩm bán được tuần này
            var soldProducts = await _dbContext.OrderItems
                .Where(oi => oi.Order.OrderDate >= startOfWeek && oi.Order.OrderDate < endOfWeek &&
                            oi.Order.Status == OrderStatus.Paid)
                .SumAsync(oi => (int?)oi.Quantity) ?? 0;

            // Sản phẩm bán được tuần trước
            var soldProductsLastWeek = await _dbContext.OrderItems
                .Where(oi => oi.Order.OrderDate >= startOfLastWeek && oi.Order.OrderDate < endOfLastWeek &&
                            oi.Order.Status == OrderStatus.Paid)
                .SumAsync(oi => (int?)oi.Quantity) ?? 0;

            // Chỉ tiêu trong tuần
            decimal revenueTarget = 10_000_000m;
            int orderTarget = 150;
            int customerTarget = 25;
            int soldTarget = 200;

            return new WeeklyStatisticDTO
            {
                Revenue = revenueThisWeek,
                RevenueChange = CalcPercent(revenueThisWeek, revenueLastWeek),
                RevenueProgress = GetProgress(revenueThisWeek, revenueTarget),
                RevenueTarget = revenueTarget,

                NewCustomers = newCustomers,
                CustomerChange = CalcPercent(newCustomers, newCustomersLastWeek),
                CustomerProgress = GetProgress(newCustomers, customerTarget),
                CustomerTarget = customerTarget,

                OrderCount = orderCount,
                OrderChange = CalcPercent(orderCount, orderCountLastWeek),
                OrderProgress = GetProgress(orderCount, orderTarget),
                OrderTarget = orderTarget,

                SoldProducts = soldProducts,
                SoldChange = CalcPercent(soldProducts, soldProductsLastWeek),
                SoldProgress = GetProgress(soldProducts, soldTarget),
                SoldTarget = soldTarget
            };
        }

        public async Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year)
        {
            await using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            if (year == 0)
                year = DateTime.Now.Year;

            var result = await _dbContext.Orders
                .Where(o => o.OrderDate.Year == year && o.Status == OrderStatus.Paid)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new RevenueByMonthDTO
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(r => r.Month)
                .ToListAsync();

            return result;
        }

        public async Task<List<RevenueByDateDTO>> GetRevenueByRangeAsync(DateTime startDate, DateTime endDate)
        {
            await using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            var data = await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.Paid &&
                           o.OrderDate.Date >= startDate.Date &&
                           o.OrderDate.Date <= endDate.Date)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueByDateDTO
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            // Điền đầy đủ các ngày trong khoảng
            var fullRange = Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
                .Select(offset => startDate.Date.AddDays(offset))
                .Select(date => new RevenueByDateDTO
                {
                    Date = date,
                    Revenue = data.FirstOrDefault(d => d.Date == date)?.Revenue ?? 0
                })
                .ToList();

            return fullRange;
        }

        public async Task<List<TopDataDTO>> GetTopDataAsync(int type, int limit, DateTime? startDate, DateTime? endDate)
        {
            await using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            if (limit <= 0) limit = 5;

            DateTime start = startDate ?? DateTime.Now.AddMonths(-1);
            DateTime end = endDate ?? DateTime.Now;

            switch (type)
            {
                case 1: // Top sản phẩm bán chạy nhất
                    return await _dbContext.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Paid &&
                                    oi.Order.OrderDate >= start && oi.Order.OrderDate <= end)
                        .GroupBy(oi => oi.Product.ProductName)
                        .Select(g => new TopDataDTO { Label = g.Key, Value = g.Sum(x => x.Quantity) })
                        .OrderByDescending(x => x.Value)
                        .Take(limit)
                        .ToListAsync();

                case 2: // Sản phẩm ít bán chạy nhất
                    return await _dbContext.OrderItems
                        .Where(oi => oi.Order.Status == OrderStatus.Paid &&
                                    oi.Order.OrderDate >= start && oi.Order.OrderDate <= end)
                        .GroupBy(oi => oi.Product.ProductName)
                        .Select(g => new TopDataDTO { Label = g.Key, Value = g.Sum(x => x.Quantity) })
                        .OrderBy(x => x.Value)
                        .Take(limit)
                        .ToListAsync();

                case 3: // Khách hàng chi tiêu nhiều nhất
                    return await _dbContext.Orders
                        .Where(o => o.Status == OrderStatus.Paid &&
                                   o.OrderDate >= start && o.OrderDate <= end)
                        .GroupBy(o => o.Customer.Name)
                        .Select(g => new TopDataDTO { Label = g.Key, Value = g.Sum(x => x.TotalAmount) })
                        .OrderByDescending(x => x.Value)
                        .Take(limit)
                        .ToListAsync();

                case 4: // Khách hàng chi tiêu ít nhất
                    return await _dbContext.Orders
                        .Where(o => o.Status == OrderStatus.Paid &&
                                   o.OrderDate >= start && o.OrderDate <= end)
                        .GroupBy(o => o.Customer.Name)
                        .Select(g => new TopDataDTO { Label = g.Key, Value = g.Sum(x => x.TotalAmount) })
                        .OrderBy(x => x.Value)
                        .Take(limit)
                        .ToListAsync();

                default:
                    return new List<TopDataDTO>();
            }
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            await using var _dbContext = await _dbContextFactory.CreateDbContextAsync();

            return await _dbContext.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        private decimal CalcPercent(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((current - previous) / previous) * 100, 1);
        }

        private decimal GetProgress(decimal value, decimal target)
        {
            if (target == 0) return 0;
            var progress = (value / target) * 100;
            return progress > 100 ? 100 : Math.Round(progress, 1);
        }
    }
}