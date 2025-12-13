using Microsoft.Extensions.Configuration;
using StoreBlazor.DTO.Payment;
using StoreBlazor.Services.Payment;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace StoreBlazor.Services.Payment.Implementations
{
    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(VNPayRequestDto request, bool isClient=false)
        {
            var vnp_TmnCode = _configuration["VnPay:TmnCode"];
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_Url = _configuration["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _configuration["VnPay:PaymentBackReturnUrl"];
            // Nếu là client thì dùng URL dành cho client
            if (isClient)
            {
                vnp_ReturnUrl = _configuration["VnPay:PaymentBackClientReturnUrl"];
            }
            // Tạo dữ liệu request
            var vnpay = new VNPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString()); // VNPay tính bằng VNDx100
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", request.OrderId);

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

        public VNPayResponseDto ProcessCallback(Dictionary<string, string> queryParams)
        {
            var vnp_HashSecret = _configuration["VnPay:HashSecret"] ?? _configuration["PaymentConfig:VNPay:HashSecret"];
            if (string.IsNullOrEmpty(vnp_HashSecret))
                throw new InvalidOperationException("Missing VNPay HashSecret configuration for callback validation.");

            var vnpay = new VNPayLibrary();

            foreach (var param in queryParams)
            {
                if (!string.IsNullOrEmpty(param.Key) && param.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(param.Key, param.Value);
                }
            }

            var vnp_SecureHash = queryParams.ContainsKey("vnp_SecureHash") ? queryParams["vnp_SecureHash"] : null;
            if (string.IsNullOrEmpty(vnp_SecureHash))
            {
                return new VNPayResponseDto { Success = false, Message = "Missing vnp_SecureHash" };
            }

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            if (!checkSignature)
            {
                return new VNPayResponseDto
                {
                    Success = false,
                    Message = "Chữ ký không hợp lệ"
                };
            }

            var responseCode = queryParams.ContainsKey("vnp_ResponseCode") ? queryParams["vnp_ResponseCode"] : "";
            var transactionStatus = queryParams.ContainsKey("vnp_TransactionStatus") ? queryParams["vnp_TransactionStatus"] : "";

            return new VNPayResponseDto
            {
                Success = responseCode == "00" && transactionStatus == "00",
                OrderId = queryParams.ContainsKey("vnp_TxnRef") ? queryParams["vnp_TxnRef"] : "",
                TransactionId = queryParams.ContainsKey("vnp_TransactionNo") ? queryParams["vnp_TransactionNo"] : "",
                Message = responseCode == "00" ? "Giao dịch thành công" : "Giao dịch thất bại",
                vnp_ResponseCode = responseCode,
                vnp_TransactionStatus = transactionStatus,
                vnp_TxnRef = queryParams.ContainsKey("vnp_TxnRef") ? queryParams["vnp_TxnRef"] : "",
                vnp_Amount = queryParams.ContainsKey("vnp_Amount") ? queryParams["vnp_Amount"] : "",
                vnp_BankCode = queryParams.ContainsKey("vnp_BankCode") ? queryParams["vnp_BankCode"] : "",
                vnp_OrderInfo = queryParams.ContainsKey("vnp_OrderInfo") ? queryParams["vnp_OrderInfo"] : ""
            };
        }
    }

    // Helper class để xử lý VNPay
    public class VNPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            string queryString = data.ToString();
            if (queryString.Length > 0)
            {
                queryString = queryString.Remove(queryString.Length - 1, 1);
            }

            string signData = queryString;
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }

        private string HmacSHA512(string key, string inputData)
        {
            if (inputData == null) inputData = string.Empty;
            if (key == null) throw new ArgumentNullException(nameof(key), "VNPay secret key is null. Check configuration.");

            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    public class VNPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}