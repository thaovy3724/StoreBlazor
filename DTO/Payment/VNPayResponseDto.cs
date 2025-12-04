namespace StoreBlazor.DTO.Payment
{
    public class VNPayResponseDto
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Dữ liệu callback từ VNPay
        public string? vnp_TmnCode { get; set; }
        public string? vnp_Amount { get; set; }
        public string? vnp_BankCode { get; set; }
        public string? vnp_BankTranNo { get; set; }
        public string? vnp_CardType { get; set; }
        public string? vnp_OrderInfo { get; set; }
        public string? vnp_PayDate { get; set; }
        public string? vnp_ResponseCode { get; set; }
        public string? vnp_TransactionNo { get; set; }
        public string? vnp_TransactionStatus { get; set; }
        public string? vnp_TxnRef { get; set; }
        public string? vnp_SecureHash { get; set; }
    }
}