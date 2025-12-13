using Microsoft.AspNetCore.Mvc;
using StoreBlazor.Models;
using StoreBlazor.Services.Client.Interfaces;
using StoreBlazor.Services.Payment;

namespace StoreBlazor.Controllers
{
    [Route("payment")]
    public class PaymentCallbackController : Controller
    {
        private readonly IVNPayService _vnPayService;
        private readonly IMoMoService _moMoService;
        private readonly IOrderService _orderService;

        public PaymentCallbackController(
            IVNPayService vnPayService,
            IMoMoService moMoService,
            IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _moMoService = moMoService;
            _orderService = orderService;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            // Tách query parameters thành dictionary
            var query = Request.Query.ToDictionary(
                q => q.Key,
                q => q.Value.ToString()
            );

            // CALLBACK TỪ VNPAY
            if (query.ContainsKey("vnp_ResponseCode"))
            {
                var vnpayResponse = _vnPayService.ProcessCallback(query);

                if (vnpayResponse.Success)
                {
                    await _orderService.UpdateOrderStatusAfterPaymentAsync(
                        int.Parse(vnpayResponse.OrderId),
                        PaymentMethod.BankTransfer
                    );

                    return Redirect($"/client/payment-success/{vnpayResponse.OrderId}");
                }
                else
                {
                    await _orderService.CancelOrderAsync(
                        int.Parse(vnpayResponse.OrderId)
                    );

                    return Redirect($"/client/payment-fail/{vnpayResponse.OrderId}");
                }
            }

            // CALLBACK TỪ MOMO
            if (query.ContainsKey("resultCode"))
            {
                var orderId = int.Parse(query["orderId"]);
                var resultCode = query["resultCode"];
                var isSuccess = resultCode == "0";

                if (isSuccess)
                {
                    await _orderService.UpdateOrderStatusAfterPaymentAsync(
                        orderId,
                        PaymentMethod.EWallet
                    );

                    return Redirect($"/client/payment-success/{orderId}");
                }
                else
                {
                    await _orderService.CancelOrderAsync(orderId);

                    return Redirect($"/client/payment-fail/{orderId}");
                }
            }

            // Nếu không đúng callback của VNPay hoặc MoMo
            return Redirect("/client");
        }
    }
}
