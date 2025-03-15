using Microsoft.AspNetCore.Mvc;

using iFood.Interfaces;
using iFood.Models;
using System.Security.Claims;
using iFood.Data.Enum;
using Newtonsoft.Json;
using System.Runtime.InteropServices.JavaScript;
using ZaloPay.Helper.Crypto;
using iFood.Models.ZaloPay;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using iFood.Helpers; // https://www.newtonsoft.com/json

namespace iFood.Controllers;
[Authorize]
public class PaymentController : Controller
{

    private readonly PaymentToggle _paymentToggle;
    public PaymentController(PaymentToggle paymentToggle)
    {
        _paymentToggle = paymentToggle;
        
    }
    [HttpGet]
    public async Task<IActionResult> CreatePayment(PaymentMethod paymentMethod)
    {
        var productToDeleteJson = HttpContext.Session.GetString("ProductToDelete");
        var orderJson = HttpContext.Session.GetString("NewOrder");

        if (string.IsNullOrEmpty(orderJson))
            return BadRequest("Order not found!");
        List<Product> products = JsonConvert.DeserializeObject<List<Product>>(productToDeleteJson);
        Order newOrder = JsonConvert.DeserializeObject<Order>(orderJson);
        OrderInfo model = new OrderInfo
        {
            AppUserId = User.GetUserId(),
            OrderId = Guid.NewGuid().ToString(),
            Amount = double.Parse((newOrder.TotalPrice * 24000).ToString()),
            FullName = User.Identity.Name,
            OrderInfomation = "Momo payment for iFood website",
        };
        if(model.Amount >= 60000000)
        {
            TempData["WarnMessage"] = "so tien hien tai qua lon khong the thuc hien giao dich";
            return RedirectToAction("Index","Cart");
        }

        HttpContext.Session.SetString("NewOrder", JsonConvert.SerializeObject(newOrder));
        HttpContext.Session.SetString("ProductToCallBack", JsonConvert.SerializeObject(products));
        var response = await _paymentToggle.Paymentstrategy(paymentMethod,model);
        
        return Json(new { payUrl = response });
    }
    public async Task<IActionResult> CreatePaymentByCart(PaymentMethod paymentMethod)
    {

        var productToDeleteJson = HttpContext.Session.GetString("ProductToDelete");
        var cartToDeleteJson = HttpContext.Session.GetString("CartToDelete");
        var orderJson = HttpContext.Session.GetString("NewOrder");

        if (string.IsNullOrEmpty(productToDeleteJson))
            return BadRequest("Product not found!");
        if (string.IsNullOrEmpty(cartToDeleteJson))
            return BadRequest("Order not found!");
        if (string.IsNullOrEmpty(orderJson))
            return BadRequest("Order not found!");


        List<Product> products = JsonConvert.DeserializeObject<List<Product>>(productToDeleteJson);
        List<Cart> carts = string.IsNullOrEmpty(cartToDeleteJson)
            ? new List<Cart>()
            : JsonConvert.DeserializeObject<List<Cart>>(cartToDeleteJson);
        Order order = JsonConvert.DeserializeObject<Order>(orderJson);

        OrderInfo model = new OrderInfo
        {
            OrderId = Guid.NewGuid().ToString(),
            Amount = double.Parse((order.TotalPrice * 24000).ToString()),
            FullName = User.Identity.Name,
            OrderInfomation = "Momo payment for iFood website",
        };
        //dieu kien payment method o day
        //var response = await _momoService.CreatePaymentMomoAsync(model);
        var response =  await _paymentToggle.Paymentstrategy(paymentMethod,model);

        HttpContext.Session.SetString("ProductToCallBack", JsonConvert.SerializeObject(products));
        HttpContext.Session.SetString("CartToCallBack", JsonConvert.SerializeObject(carts));
        HttpContext.Session.SetString("OrderToCallBack", JsonConvert.SerializeObject(order));
        if (Response.HasStarted)
        {
            Console.WriteLine("Lỗi: Headers đã gửi, không thể redirect!");
        }
        return Json(new { payUrl = response });
    }
}
