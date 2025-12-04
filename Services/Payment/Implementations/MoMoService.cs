using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StoreBlazor.DTO.Payment;
using StoreBlazor.Services.Payment;
using System.Security.Cryptography;
using System.Text;

namespace StoreBlazor.Services.Payment.Implementations
{
    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MoMoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private string GetConfigValue(params string[] keys)
        {
            foreach (var k in keys)
            {
                var v = _configuration[k];
                if (!string.IsNullOrEmpty(v)) return v;
            }
            throw new InvalidOperationException($"Missing MoMo configuration. Searched keys: {string.Join(", ", keys)}");
        }

        public async Task<MoMoResponseDto> CreatePaymentAsync(MoMoRequestDto request)
        {
            var partnerCode = GetConfigValue("MoMo:PartnerCode", "PaymentConfig:MoMo:PartnerCode");
            var accessKey = GetConfigValue("MoMo:AccessKey", "PaymentConfig:MoMo:AccessKey");
            var secretKey = GetConfigValue("MoMo:SecretKey", "PaymentConfig:MoMo:SecretKey");
            var endpoint = GetConfigValue("MoMo:Endpoint", "PaymentConfig:MoMo:Endpoint");
            var returnUrl = GetConfigValue("MoMo:ReturnUrl", "PaymentConfig:MoMo:ReturnUrl");
            var ipnUrl = GetConfigValue("MoMo:IpnUrl", "PaymentConfig:MoMo:IpnUrl");

            var requestId = Guid.NewGuid().ToString();
            var orderId = request.OrderId;
            var amount = request.Amount.ToString("0");
            var orderInfo = request.OrderInfo;
            var requestType = "captureWallet";
            var extraData = "";

            // Tạo chữ ký
            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSHA256(rawHash, secretKey);

            // Tạo request body
            var requestBody = new
            {
                partnerCode,
                partnerName = "Store Management",
                storeId = "StoreTest",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                lang = "vi",
                extraData,
                requestType,
                signature
            };

            try
            {
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var momoResponse = JsonConvert.DeserializeObject<MoMoResponseDto>(responseContent);

                if (momoResponse != null && momoResponse.resultCode == "0")
                {
                    momoResponse.Success = true;
                    momoResponse.Message = "Tạo link thanh toán thành công";
                }
                else
                {
                    momoResponse.Success = false;
                    momoResponse.Message = "Tạo link thanh toán thất bại: " + momoResponse?.resultCode;
                }

                return momoResponse ?? new MoMoResponseDto { Success = false, Message = "Lỗi không xác định" };
            }
            catch (Exception ex)
            {
                return new MoMoResponseDto
                {
                    Success = false,
                    Message = $"Lỗi kết nối MoMo: {ex.Message}"
                };
            }
        }

        public bool ValidateSignature(Dictionary<string, string> data, string signature)
        {
            var secretKey = _configuration["MoMo:SecretKey"] ?? _configuration["PaymentConfig:MoMo:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Missing MoMo SecretKey configuration for signature validation.");

            var rawHash = $"accessKey={data["accessKey"]}&amount={data["amount"]}&extraData={data["extraData"]}&message={data["message"]}&orderId={data["orderId"]}&orderInfo={data["orderInfo"]}&orderType={data["orderType"]}&partnerCode={data["partnerCode"]}&payType={data["payType"]}&requestId={data["requestId"]}&responseTime={data["responseTime"]}&resultCode={data["resultCode"]}&transId={data["transId"]}";

            var computedSignature = HmacSHA256(rawHash, secretKey);

            return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSHA256(string inputData, string key)
        {
            if (inputData == null) inputData = string.Empty;
            if (key == null) throw new ArgumentNullException(nameof(key), "MoMo secret key is null. Check configuration.");

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}