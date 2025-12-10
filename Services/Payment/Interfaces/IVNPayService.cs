using StoreBlazor.DTO.Payment;

namespace StoreBlazor.Services.Payment
{
    public interface IVNPayService
    {
        /// Tạo URL thanh toán VNPay
        string CreatePaymentUrl(VNPayRequestDto request, bool isClient=false);

        /// Xác thực callback từ VNPay
        VNPayResponseDto ProcessCallback(Dictionary<string, string> queryParams);
    }
}