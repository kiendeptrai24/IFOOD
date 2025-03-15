using iFood.Models;
using iFood.Models.ZaloPay;
using iFood.Service;


namespace iFood.Interfaces;

public interface IZaloPayService
{
    Task<string> CreatePaymentZaloPayAsync(OrderInfo model);
    Task<ZaloPayExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
}
