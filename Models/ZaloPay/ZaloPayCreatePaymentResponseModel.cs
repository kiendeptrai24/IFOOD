namespace iFood.Models.ZaloPay
{
    public class ZaloPayCreatePaymentResponseModel
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string PayUrl { get; set; }
        public string ZpTransToken { get; set; }
    }
}
