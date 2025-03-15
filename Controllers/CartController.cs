using Microsoft.AspNetCore.Mvc;
using iFood.Models;
using iFood.Interfaces;
using iFood.ViewModels;
using iFood.Data.Enum;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using iFood.Data;

namespace iFood.Controllers;

public class CartController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;

    public CartController( UserManager<AppUser> userManager,IProductRepository productRepository, ICartRepository cartRepository)
    {
        _userManager = userManager;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
    }

    // public async Task<IActionResult> Index()
    // {
    //     IEnumerable<Product> products = await _productRepository.GetAll();
    //     return View(products);
    // }
    public async Task<IActionResult> Index()
    {

        List<Cart> carts = await _cartRepository.GetAllByUserId();
        IndexCartViewModel cartVM = new IndexCartViewModel
        {
            CartItems = carts,
        };
        
        return View(cartVM);
    }
    public async Task<IActionResult> Detail(int id)
    {
        Cart cart = await _cartRepository.GetByIdAsync(id);
        return View(cart);

    }
    [HttpGet]
    public async Task<IActionResult> Create(int id)
    {
        if(!User.Identity.IsAuthenticated)
        {
            TempData["SuccessMessage"] = "should be login account!";
            return RedirectToAction("Login","Account");
        }
        var curUser = await HttpContext.User.GetUserById(_userManager);
        var curUserId = HttpContext.User.GetUserId();
        
        var productToCart = await _productRepository.GetByIdAsync(id);

        var productIncart = await _cartRepository.GetCartByProductIdOfIdentificationUserAsync(id);
        
        if (productToCart == null)
        {
            TempData["ErrorMessage"] = "Don't have the Product like this!";
        }
        if(productIncart != null)
        {
            productIncart.Quantity++;
            _cartRepository.Update(productIncart);
            TempData["SuccessMessage"] = "increase " + productToCart.Name + " to " + productIncart.Quantity +" at your cart";
        }
        else
        {
            var cart = new Cart
            {
                AppUserId = curUserId,
                AppUser = curUser,
                ProductId = productToCart.ProductID,
                product = productToCart,
                Quantity = 1,
                Price = productToCart.Price,
            };
            if(cart != null)
            {
                productToCart.Category--;
                _productRepository.Update(productToCart);
            }
            TempData["SuccessMessage"] = "Add new product to cart complete!";

            _cartRepository.Add(cart);
        }

        return RedirectToAction("Index","Home");
    }
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return View("Error");
        var productVM = new EditProductViewModel
        {
            Title = product.Name,
            Description = product.Description,
            Date = product.DateSell,
            Quantity = product.Quantity,
            Price = product.Price,
            Category = product.Category,
            URL = product.Image
        };
        
        return View(productVM);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditProductViewModel productVM)
    {
        // if (!ModelState.IsValid)
        // {
        //     ModelState.AddModelError("", "Failed to edit club");
        //     return View(productVM);
        // }

        // var userProduct = await _productRepository.GetByIdAsyncNoTracking(id);

        // if (userProduct == null)
        // {
        //     return View("Error");   
        // }
        // Product product;
        // if(productVM.Image !=  null)
        // {
        //     var photoResult = await _photoService.AddPhotoAsync(productVM.Image);

        //     if (photoResult.Error != null)
        //     {
        //         ModelState.AddModelError("Image", "Photo upload failed");
        //         return View(productVM);
        //     }
        //     if (!string.IsNullOrEmpty(userProduct.Image))
        //     {
        //         var photo = _photoService.DeletePhotoAsync(userProduct.Image);
        //     }
        //     product = new Product
        //     {
        //         ProductID = id,
        //         Name = productVM.Title,
        //         Description = productVM.Description,
        //         DateSell = productVM.Date,
        //         Image = photoResult.Url.ToString(),
        //         URL = productVM.URL,
        //         Category = productVM.Category,
        //         Quantity = productVM.Quantity,
        //         Price = productVM.Price,
        //     };
        // }
        // else{
        //     product = new Product
        //     {
        //         ProductID = id,
        //         Name = productVM.Title,
        //         Description = productVM.Description,
        //         DateSell = productVM.Date,
        //         Image = productVM.URL,
        //         Category = productVM.Category,
        //         Quantity = productVM.Quantity,
        //         Price = productVM.Price,
        //     };
        // }




        // _productRepository.Update(product);

        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var productToDelete = await _cartRepository.GetByIdAsync(id);

        // logic huy don hang

        // var productToRetoreQuantity = productToDelete.product;
        // if(productToRetoreQuantity == null)
        // {
        //     TempData["WarnMessage"] = "Don't have the product to retore!";
        //     _cartRepository.Delete(productToDelete);
        //     return RedirectToAction("Index","Cart");
        // }
        // productToRetoreQuantity.Quantity += productToDelete.Quantity;

        // _productRepository.Update(productToRetoreQuantity);



        if (productToDelete == null)
        {
            TempData["ErrorMessage"] = "Don't have the product like this!";
            return RedirectToAction("Index","Cart");
        }


        _cartRepository.Delete(productToDelete);
        TempData["SuccessMessage"] = "The product has been removed from your cart!";

        
        return RedirectToAction("Index","Cart");
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // var productDetails = await _productRepository.GetByIdAsync(id);

        // if (productDetails == null)
        // {
        //     return View("Error");
        // }

        // if (!string.IsNullOrEmpty(productDetails.Image))
        // {
        //     _ = _photoService.DeletePhotoAsync(productDetails.Image);
        // }

        // _productRepository.Delete(productDetails);
        return RedirectToAction("Index");
    }
}
