using Microsoft.AspNetCore.Mvc;

using iFood.Models;
using iFood.Data.Enum;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using iFood.Helpers;
using iFood.Models.Momo; // https://www.newtonsoft.com/json

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
        if (string.IsNullOrEmpty(productToDeleteJson))
            return BadRequest("Product not found!");
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
        if(model.Amount <= 0)
        {
            TempData["WarnMessage"] = "so tien khong hop le";
            return RedirectToAction("Index","Cart");
        }
        var response = await _paymentToggle.Paymentstrategy(paymentMethod,model);

        HttpContext.Session.SetString("ProductToCallBack", JsonConvert.SerializeObject(products));
        HttpContext.Session.SetString("OrderToCallBack", JsonConvert.SerializeObject(newOrder));

        // If payment strategy returns COD, return the callback URL so frontend can navigate to finalize the COD order
        if (string.Equals(response, "COD", StringComparison.OrdinalIgnoreCase))
        {
            var codUrl = Url.Action("SaveCOD", "Callback");
            return Json(new { payUrl = codUrl });
        }

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
        var response = await _paymentToggle.Paymentstrategy(paymentMethod, model);
        HttpContext.Session.SetString("ProductToCallBack", JsonConvert.SerializeObject(products));
        HttpContext.Session.SetString("CartToCallBack", JsonConvert.SerializeObject(carts));
        HttpContext.Session.SetString("OrderToCallBack", JsonConvert.SerializeObject(order));
        string message = "";
        MomoCreatePaymentResponseModel result;
        if (string.Equals(response, "COD", StringComparison.OrdinalIgnoreCase))
        {
            var codUrl = Url.Action("SaveCOD", "Callback");
            return Json(new { payUrl = codUrl });
        }
        else
        {
            result = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response);
            message = result.Message;
            if(message != "Success")
            {
                TempData["Error"] = "Error MoMo: " + message;
            }

        }       
        if (Response.HasStarted)
        {
            Console.WriteLine("Lỗi: Headers đã gửi, không thể redirect!");
        }
        return Json(new { payUrl = result.PayUrl });
    }
}
