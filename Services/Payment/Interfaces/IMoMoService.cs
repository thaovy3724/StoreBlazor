using StoreBlazor.DTO.Payment;

namespace StoreBlazor.Services.Payment
{
    public interface IMoMoService
    {
        /// Tạo request thanh toán MoMo
        Task<MoMoResponseDto> CreatePaymentAsync(MoMoRequestDto request, bool isClient=false);

        /// Xác thực IPN callback từ MoMo
        bool ValidateSignature(Dictionary<string, string> data, string signature);
    }
}