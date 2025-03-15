using iFood.Models;
using iFood.Models.Momo;

namespace iFood.Interfaces;

public interface IMomoService 
{
    Task<string> CreatePaymentMomoAsync(OrderInfo model);
    Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
}
