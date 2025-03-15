using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using iFood.Models.ZaloPay;
using iFood.Models;
using iFood.Interfaces;
using ZaloPay.Helper;
using ZaloPay.Helper.Crypto;
using Newtonsoft.Json;

namespace iFood.Service
{
    // public class BankListResponse {
    //     public string returncode { get; set; }
    //     public string returnmessage { get; set; }
    //     public Dictionary<string, List<BankDTO>> banks { get; set; }
    // }
    // public class BankDTO {
    //         public string bankcode { get; set; }
    //         public string name { get; set; }
    //         public int displayorder { get; set; }
    //         public int pmcid { get; set; }
    //     }
    public class ZaloPayService : IZaloPayService
    {
        private readonly IOptions<ZaloPayOptionModel> _options;
        public ZaloPayService(IOptions<ZaloPayOptionModel> options)
        {
            _options = options;
        }

        public async Task<string> CreatePaymentZaloPayAsync(OrderInfo model)
        {

            Random rnd = new Random();
            var items = new[]{new{}};
            var app_trans_id   = rnd.Next(1000000); // Generate a random order's ID.
            var param = new Dictionary<string, string>();    
            var ZaloPayUrl =_options.Value.ZaloPayUrl;
            var app_id =_options.Value.AppId;
            var key1 = _options.Value.Key1;
            var embed_data = new Dictionary<string, object>
            {
                { "redirecturl", _options.Value.NotifyUrl }
            };
            
            param.Add("app_id", app_id);
            param.Add("app_user", model.AppUserId);
            param.Add("app_time", Utils.GetTimeStamp().ToString());
            param.Add("amount", "50000");
            param.Add("app_trans_id", DateTime.Now.ToString("yyMMdd") + "_" + app_trans_id); // mã giao dich có định dạng yyMMdd_xxxx
            param.Add("embed_data", JsonConvert.SerializeObject(embed_data));
            param.Add("item", JsonConvert.SerializeObject(items));
            param.Add("description", "Lazada - Thanh toán đơn hàng #"+ app_trans_id);
            param.Add("callback_url", _options.Value.ReturnUrl);
           


            //param.Add("bank_code", "zalopayapp");

            var data = app_id + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|" 
                + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];



            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key1, data));

            var result = await HttpHelper.PostFormAsync(ZaloPayUrl, param);

            return result["order_url"].ToString();
            
        }

        private string ComputeHmacSha256(string data, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public async Task<ZaloPayExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection["amount"];
            var orderInfo = collection["description"];
            var orderId = collection["apptransid"];

            return new ZaloPayExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

    }
}
