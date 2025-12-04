namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO cho autocomplete tìm kiếm khách hàng
    /// </summary>
    public class CustomerSuggestionDto
    {
        public int CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Address { get; set; }
    }
}