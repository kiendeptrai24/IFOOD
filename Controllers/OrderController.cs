using Microsoft.AspNetCore.Mvc;

using iFood.Interfaces;
using iFood.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using iFood.ViewModels;
using iFood.Data.Enum;

namespace iFood.Controllers;

public class OrderController : Controller
{
    struct Notice
    {
        public string Name;
        public int Quantity;
    }
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICartRepository _cartRepository;
    public OrderController(UserManager<AppUser> userManager,IOrderRepository orderRepository,IProductRepository productRepository,ICartRepository cartRepository)
    {
        _userManager = userManager;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
    }
    public async Task<IActionResult> Index()
    {

        var order = await _orderRepository.GetAllByUserId();

        return View(order);
    }
    public async Task<IActionResult> OrderDetail(int id)
    {

        var order =await _orderRepository.GetByIdAsync(id);
        return View(order);
    }
    
    
    public async Task<IActionResult> UpdateStateForOrder(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound("Order not found");
        }
        order.status = GetNextStatus(order.status);
        _orderRepository.Update(order);
        return RedirectToAction("OrderIndex","Dashboard");
    }
   private Status GetNextStatus(Status currentStatus)
{
    return currentStatus switch
    {
        Status.PendingConfirmation => Status.Shipping,
        Status.Shipping => Status.Delivered,
        Status.Delivered => Status.Completed,
        Status.Completed => Status.Completed, // Không thay đổi nếu đã hoàn thành
        Status.Canceled => Status.Canceled, // Đã hủy thì không thay đổi
        Status.ReturnedOrRefunded => Status.ReturnedOrRefunded, // Đã hoàn tiền thì không thay đổi
        _ => currentStatus
    };
}
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] SelectedProductViewModel model)
    {
        if(!User.Identity.IsAuthenticated)
        {
            TempData["WarnMessage"] = "Please sign in before";
            RedirectToAction("Login", "Account");
        }
        var product = await _productRepository.GetByIdAsyncNoTracking(model.productId);
        if(product.Quantity <= 0)
        {
            TempData["InfoMessage"] = "Product sold out!";
            return RedirectToAction("Index", "Home");
        }
        product.Quantity = 1; 
        
        
        var curUserId = HttpContext.User.GetUserId();

        var newOrder = new Order
        {
            AppUserId = curUserId,
            OrderDate = DateTime.Now,
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail()
                {
                    ProductId = product.ProductID,
                    Product = product,
                    Quantity = product.Quantity,
                }
            }
        };

        List<Product> products = new List<Product>();
        products.Add(product);
        HttpContext.Session.SetString("ProductToDelete", JsonConvert.SerializeObject(products));
        HttpContext.Session.SetString("NewOrder", JsonConvert.SerializeObject(newOrder));
        
        return RedirectToAction("CreatePayment", "Payment",new { paymentMethod = model.paymentMethod });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrderWithCart([FromBody] SelectedCartViewModel model)
    {

        if (model == null || model.CartIds == null || !model.CartIds.Any())
        {
            return BadRequest(new { message = "Không có sản phẩm nào được chọn!" });
        }

        // get carts of user and id user
        var curUserId = HttpContext.User.GetUserId();
        List<Cart> cartsToOrder = new List<Cart>();
        
        cartsToOrder.AddRange(await _cartRepository.GetCartsByIdNoTracking(model.CartIds));
        

        var newOrder = new Order
        {
            AppUserId = curUserId,
            OrderDate = DateTime.Now,
        };

        List<Product> productsToDeletes = new List<Product>();

        foreach (var cart in cartsToOrder)
        {
            var productToDelete = await _productRepository.GetByIdAsyncNoTracking(cart.ProductId);

            if(productToDelete != null)
            {
                
                int quantity = productToDelete.Quantity - cart.Quantity;

                if(quantity <= 0)
                {
                    productToDelete.Quantity = 0;
                    productsToDeletes.Add(productToDelete);
                    // _productRepository.Update(productToDelete);
                }
                else
                {
                    productToDelete.Quantity = cart.Quantity;
                    productsToDeletes.Add(productToDelete);
                }
            }
        }
        var newOrderDetails = new List<OrderDetail>();

        foreach (var product in productsToDeletes)
        {
           // _productRepository.CheckAttackProduct(product);
            OrderDetail orderdetail = new OrderDetail()
            {
                ProductId = product.ProductID,
                Product = product,
                Quantity = product.Quantity,
            };
            
            newOrderDetails.Add(orderdetail);
        }
        newOrder.OrderDetails.AddRange(newOrderDetails);

        //_cartRepository.DeleteAll(cartsToDeleteFromProduct);
        
        HttpContext.Session.SetString("ProductToDelete", JsonConvert.SerializeObject(productsToDeletes));
        HttpContext.Session.SetString("CartToDelete", JsonConvert.SerializeObject(cartsToOrder));
        HttpContext.Session.SetString("NewOrder", JsonConvert.SerializeObject(newOrder));


        return RedirectToAction("CreatePaymentByCart", "Payment",new { paymentMethod = model.paymentMethod });
    }

    
}
