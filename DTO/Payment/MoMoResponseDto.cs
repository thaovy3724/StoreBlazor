namespace StoreBlazor.DTO.Payment
{
    public class MoMoResponseDto
    {
        public bool Success { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        //public string QrCodeUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Dữ liệu từ MoMo API
        public string? partnerCode { get; set; }
        public string? orderId { get; set; }
        public string? requestId { get; set; }
        public long amount { get; set; }
        public long responseTime { get; set; }
        public string? resultCode { get; set; }
        public string? payType { get; set; }
        public string? signature { get; set; }
    }
}