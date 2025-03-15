using Microsoft.Extensions.Options;
using iFood.Interfaces;
using iFood.Models.Momo;
using iFood.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
namespace iFood.Service;

public class MomoService : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;
    public MomoService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }
    public async Task<string> CreatePaymentMomoAsync(OrderInfo model)
    {
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfomation = "Customer: " + model.FullName + ". Content: " + model.OrderInfomation;

        var rawData =
            $"partnerCode={_options.Value.PartnerCode}" +
            $"&accessKey={_options.Value.AccessKey}" +
            $"&requestId={model.OrderId}" +
            $"&amount={model.Amount}" +
            $"&orderId={model.OrderId}" +
            $"&orderInfo={model.OrderInfomation}" +
            $"&returnUrl={_options.Value.ReturnUrl}" +
            $"&notifyUrl={_options.Value.NotifyUrl}" +
            $"&extraData=";

        var signature = MomoComputeHmacSha256(rawData, _options.Value.SecretKey);

        var client = new RestClient(_options.Value.MomoApiUrl);
        var request = new RestRequest() { Method = Method.Post };
        request.AddHeader("Content-Type", "application/json; charset=UTF-8");

        // Create an object representing the request data
        var requestData = new 
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.Amount.ToString(),
            orderInfo = model.OrderInfomation,
            requestId = model.OrderId,
            extraData = "",
            signature = signature
        };

        request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

        var response = await client.ExecuteAsync(request);

        
        var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

        
        return momoResponse.PayUrl;
    }
    private string MomoComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
    public async Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
    {
        var amount = collection.First(s => s.Key == "amount").Value;
        var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
        var orderId = collection.First(s => s.Key == "orderId").Value;

        return new MomoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo

        };
    }
}
