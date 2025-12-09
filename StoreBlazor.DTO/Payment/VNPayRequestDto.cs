namespace StoreBlazor.DTO.Payment
{
    public class VNPayRequestDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}