namespace StoreBlazor.DTO.Admin
{
    public class PageResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; }
    }
}
