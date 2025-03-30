
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using iFood.Models;
using iFood.Interfaces;
using System.Security.Claims;
using iFood.Data.Enum;
using System.Text.Json;
using iFood.Models.ZaloPay;
using Microsoft.Extensions.Options;
using ZaloPay.Helper.Crypto;
namespace iFood.Controllers
{
    public class CallbackController: ControllerBase
    {
        private readonly IOptions<ZaloPayOptionModel> _options;
        private readonly IMomoService _momoService;
        private readonly IMomoRepository _momoRepository;
        private readonly IZaloPayService _zaloPayService;
        public readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        
        public CallbackController(
            IOptions<ZaloPayOptionModel> options,
            IMomoService momoService,IMomoRepository momoRepository
        ,IZaloPayService zaloPayService, IProductRepository productRepository, 
        IOrderRepository orderRepository, ICartRepository cartRepository
        )
        {
            _momoService = momoService;
            _zaloPayService = zaloPayService;
            _momoRepository = momoRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallBack()
        {
            var productToDeleteJson = HttpContext.Session.GetString("ProductToCallBack");
            var cartToDeleteJson = HttpContext.Session.GetString("CartToCallBack");
            var orderJson = HttpContext.Session.GetString("OrderToCallBack");

            var requestQuery = HttpContext.Request.Query;

            if (string.IsNullOrEmpty(productToDeleteJson))
                return BadRequest("Product not found!");
            if (string.IsNullOrEmpty(orderJson))
                return BadRequest("Order not found!");
            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(productToDeleteJson);
            List<Cart> carts = string.IsNullOrEmpty(cartToDeleteJson)
                ? new List<Cart>()
                : JsonConvert.DeserializeObject<List<Cart>>(cartToDeleteJson);
            Order order = JsonConvert.DeserializeObject<Order>(orderJson);

            var response = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            
            foreach(var item in order.OrderDetails)
            {
                _productRepository.CheckAttackProduct(item.Product);
            }
            await _orderRepository.AddAsync(order);
            if(requestQuery["resultCode"] != 0)
            {
                order.TransactionId = requestQuery["transId"];
                order.PaymentMethod = PaymentMethod.Momo;
                order.Ordercode = requestQuery["orderId"];
                order.status = Status.PendingConfirmation;
                _orderRepository.Update(order);

                if(carts.Any())
                {
                    foreach (var cart in carts)
                    {
                        _cartRepository.Delete(cart);
                        
                    }
                }
                foreach (var product in products)
                {
                    Product productToUpdate = await _productRepository.GetByIdAsync(product.ProductID);
                    productToUpdate.Quantity -= product.Quantity;
                    _productRepository.Update(productToUpdate);
                }
                _productRepository.UnTracking();
                
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
            return RedirectToAction("Index","Home");
        }

        [HttpPost]
        public async Task<IActionResult> PaymentCallBackZalo([FromBody] dynamic cbdata)
        {
            var productToDeleteJson = HttpContext.Session.GetString("ProductToCallBack");
            var cartToDeleteJson = HttpContext.Session.GetString("CartToCallBack");
            var orderJson = HttpContext.Session.GetString("OrderToCallBack");

            if (string.IsNullOrEmpty(productToDeleteJson))
                return BadRequest("Product not found!");
            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(productToDeleteJson);
            List<Cart> carts = string.IsNullOrEmpty(cartToDeleteJson)
                ? new List<Cart>()
                : JsonConvert.DeserializeObject<List<Cart>>(cartToDeleteJson);
            Order order = string.IsNullOrEmpty(orderJson)
                ? new Order()
                : JsonConvert.DeserializeObject<Order>(orderJson);

            var result = new Dictionary<string, object>();
            try
            {
                cbdata.TryGetProperty("data", out JsonElement dataElement);
                cbdata.TryGetProperty("mac", out JsonElement macElement);

                var dataStr = dataElement.GetString();
                var reqMac = macElement.GetString();

                Console.WriteLine("Received data: " + dataStr);
                Console.WriteLine("Received MAC: " + reqMac);
                Console.WriteLine("Key2 from config: " + _options.Value.Key2);

                // Xóa khoảng trắng nếu có
                // dataStr = dataStr.Trim();

                // Tính lại MAC
                var mac = HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, _options.Value.Key2, dataStr);
                Console.WriteLine("Computed MAC: " + mac);

                // So sánh MAC nhận từ ZaloPay với MAC tính toán được
                if (!reqMac.Equals(mac))
                {
                    Console.WriteLine("MAC mismatch!");
                    result["return_code"] = -1;
                    result["return_message"] = "mac not equal";
                }
                else
                {
                    var dataJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
                    Console.WriteLine("update order's status = success where app_trans_id = {0}", dataJson["app_trans_id"]);


                    foreach(var item in order.OrderDetails)
                    {
                        _productRepository.CheckAttackProduct(item.Product);
                    }
                    await _orderRepository.AddAsync(order);
                    
                    order.TransactionId = dataJson["app_trans_id"].ToString();
                    order.PaymentMethod = PaymentMethod.Zalopay;
                    order.Ordercode = Guid.NewGuid().ToString();
                    order.OrderDate = DateTime.Now;
                    order.status = Status.PendingConfirmation;
                    _orderRepository.Update(order);

                    if(carts.Any())
                    {
                        foreach (var cart in carts)
                        {
                            _cartRepository.Delete(cart);
                            
                        }
                    }
                    foreach (var product in products)
                    {
                        Product productToUpdate = await _productRepository.GetByIdAsync(product.ProductID);
                        productToUpdate.Quantity = product.Quantity;
                        _productRepository.Update(productToUpdate);
                    }
                    _productRepository.UnTracking();


                    result["return_code"] = 1;
                    result["return_message"] = "success";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                result["return_code"] = 0;
                result["return_message"] = ex.Message;
            }

            return Ok(result);
        }

        
    }
}