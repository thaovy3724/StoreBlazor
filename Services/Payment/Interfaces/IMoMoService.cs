using StoreBlazor.DTO.Payment;

namespace StoreBlazor.Services.Payment.Interfaces
{
    public interface IMoMoService
    {
        /// Tạo request thanh toán MoMo
        Task<MoMoResponseDto> CreatePaymentAsync(MoMoRequestDto request);

        /// Xác thực IPN callback từ MoMo
        bool ValidateSignature(Dictionary<string, string> data, string signature);
    }
}