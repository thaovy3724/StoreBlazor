using StoreBlazor.DTO.Statistic;

namespace StoreBlazor.Services.Admin.Interfaces
{
    public interface IStatisticService
    {
        Task<WeeklyStatisticDTO> GetWeeklyStatisticsAsync();

        Task<List<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year);

        Task<List<RevenueByDateDTO>> GetRevenueByRangeAsync(DateTime startDate, DateTime endDate);

        Task<List<TopDataDTO>> GetTopDataAsync(int type, int limit, DateTime? startDate, DateTime? endDate);

        Task<List<int>> GetAvailableYearsAsync();
    }
}
