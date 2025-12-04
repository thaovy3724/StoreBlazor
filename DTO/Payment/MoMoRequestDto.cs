namespace StoreBlazor.DTO.Payment
{
    public class MoMoRequestDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
    }
}