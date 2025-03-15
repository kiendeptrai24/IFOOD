

using iFood.Data.Enum;
using iFood.Interfaces;
using iFood.Models;

namespace iFood.Helpers;

public class PaymentToggle
{
    public readonly IMomoService _momoService;
    public readonly IZaloPayService _zaloPayService;
    public PaymentToggle(IMomoService momoService, IZaloPayService zaloPayService)
    {
        _momoService = momoService;
        _zaloPayService = zaloPayService;
    }
    public  async Task<string> Paymentstrategy(PaymentMethod paymentMethod, OrderInfo info)
    {
        switch(paymentMethod)
        {
            case PaymentMethod.COD:
                return "COD";
            case PaymentMethod.Zalopay:
                return await _zaloPayService.CreatePaymentZaloPayAsync(info);
            case PaymentMethod.Momo:
                return await _momoService.CreatePaymentMomoAsync(info);
            default:
                return "";
        }
    }
}
