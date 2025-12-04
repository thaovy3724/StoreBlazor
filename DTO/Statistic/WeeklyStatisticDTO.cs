namespace StoreBlazor.DTO.Statistic
{
    public class WeeklyStatisticDTO
    {
        public decimal Revenue { get; set; }
        public decimal RevenueChange { get; set; }
        public decimal RevenueProgress { get; set; }
        public decimal RevenueTarget { get; set; }

        public int NewCustomers { get; set; }
        public decimal CustomerChange { get; set; }
        public decimal CustomerProgress { get; set; }
        public int CustomerTarget { get; set; }

        public int OrderCount { get; set; }
        public decimal OrderChange { get; set; }
        public decimal OrderProgress { get; set; }
        public int OrderTarget { get; set; }

        public int SoldProducts { get; set; }
        public decimal SoldChange { get; set; }
        public decimal SoldProgress { get; set; }
        public int SoldTarget { get; set; }
    }
}
