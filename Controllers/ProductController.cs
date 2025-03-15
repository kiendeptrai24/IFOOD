using Microsoft.AspNetCore.Mvc;
using iFood.Models;
using iFood.Interfaces;
using iFood.ViewModels;
using iFood.Data.Enum;
using CloudinaryDotNet.Actions;

namespace iFood.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly IPhotoService _photoService;

    public ProductController(IProductRepository productRepository,IPhotoService photoService)
    {
        _productRepository = productRepository;
        _photoService = photoService;
    }

    // public async Task<IActionResult> Index()
    // {
    //     IEnumerable<Product> products = await _productRepository.GetAll();
    //     return View(products);
    // }
    public async Task<IActionResult> Index(string searchTerm,int category = -1, int page = 1, int pageSize = 6)
    {
        var productViewModel = new IndexProductViewModel();
        if (page < 1 || pageSize < 1)
        {
            return NotFound();
        }
       
        // if category is -1 (All) dont filter else filter by selected category
        var products = category switch
        {
            -1 => await _productRepository.GetSliceAsync((page - 1) * pageSize, pageSize),
            _ => await _productRepository.GetProductsByCategoryAndSliceAsync((ProductCategory)category, (page - 1) * pageSize, pageSize),
        };
        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = await _productRepository.SearchAsync(searchTerm);
        }
        var count = category switch
        {
            -1 => await _productRepository.GetCountAsync(),
            _ => await _productRepository.GetCountByCategoryAsync((ProductCategory)category),
        };

        productViewModel = new IndexProductViewModel
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalProducts = count,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Category = category,
        };
        

        
        return View(productViewModel);
    }
    public async Task<IActionResult> Detail(int id)
    {
        Product product = await _productRepository.GetByIdAsync(id);
        return View(product);

    }
    
    public IActionResult Create()
    {
        var curUserId = HttpContext.User.GetUserId();
        var createRaceViewModel = new CreateProductViewModel { AppUserId = curUserId };
        return View(createRaceViewModel);
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductViewModel productVM)
    {
        if(ModelState.IsValid)
        {
            var result = await _photoService.AddPhotoAsync(productVM.Image);
            var product = new Product
            {
                Name = productVM.Title,
                Description = productVM.Description,
                Category = productVM.Category,
                DateSell = productVM.Date,
                URL = productVM.URL,
                Image = result.Url.ToString(),
                AppUserId = productVM.AppUserId,
                Price = productVM.Price,
                Quantity = productVM.Quantity,
            };
            TempData["SuccessMessage"] = "added "+productVM.Title+" to management";
            _productRepository.Add(product);
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "Photo upload failed");
            TempData["ErrorMessage"] = "Photo upload failed";

            return View(productVM);
        }
        
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
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Failed to edit club");
            return View(productVM);
        }

        var userProduct = await _productRepository.GetByIdAsyncNoTracking(id);

        if (userProduct == null)
        {
            return View("Error");   
        }
        Product product;
        if(productVM.Image !=  null)
        {
            var photoResult = await _photoService.AddPhotoAsync(productVM.Image);

            if (photoResult.Error != null)
            {
                ModelState.AddModelError("Image", "Photo upload failed");
                return View(productVM);
            }
            if (!string.IsNullOrEmpty(userProduct.Image))
            {
                var photo = _photoService.DeletePhotoAsync(userProduct.Image);
            }
            product = new Product
            {
                ProductID = id,
                Name = productVM.Title,
                Description = productVM.Description,
                DateSell = productVM.Date,
                Image = photoResult.Url.ToString(),
                URL = productVM.URL,
                Category = productVM.Category,
                Quantity = productVM.Quantity,
                Price = productVM.Price,
            };
        }
        else{
            product = new Product
            {
                ProductID = id,
                Name = productVM.Title,
                Description = productVM.Description,
                DateSell = productVM.Date,
                Image = productVM.URL,
                Category = productVM.Category,
                Quantity = productVM.Quantity,
                Price = productVM.Price,
            };
        }




        _productRepository.Update(product);

        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var productDetails = await _productRepository.GetByIdAsync(id);
        if (productDetails == null) return View("Error");
        return View(productDetails);
    }
    [HttpGet]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var productDetails = await _productRepository.GetByIdAsync(id);

        if (productDetails == null)
        {
            return View("Error");
        }

        if (!string.IsNullOrEmpty(productDetails.Image))
        {
            _ = _photoService.DeletePhotoAsync(productDetails.Image);
        }

        _productRepository.Delete(productDetails);
        return RedirectToAction("Index","Dashboard");
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var productDetails = await _productRepository.GetByIdAsync(id);

        if (productDetails == null)
        {
            return View("Error");
        }

        if (!string.IsNullOrEmpty(productDetails.Image))
        {
            _ = _photoService.DeletePhotoAsync(productDetails.Image);
        }

        _productRepository.Delete(productDetails);
        return RedirectToAction("Index");
    }

    
}
