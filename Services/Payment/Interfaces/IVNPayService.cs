using StoreBlazor.DTO.Payment;

namespace StoreBlazor.Services.Payment.Interfaces
{
    public interface IVNPayService
    {
        /// Tạo URL thanh toán VNPay
        string CreatePaymentUrl(VNPayRequestDto request);

        /// Xác thực callback từ VNPay
        VNPayResponseDto ProcessCallback(Dictionary<string, string> queryParams);
    }
}