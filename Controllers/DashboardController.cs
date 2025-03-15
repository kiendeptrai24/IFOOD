using Microsoft.AspNetCore.Mvc;
using iFood.Models;
using iFood.Interfaces;
using iFood.ViewModels;
using iFood.Data.Enum;

namespace iFood.Controllers;

public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly IPhotoService _photoService;

    public DashboardController(IProductRepository productRepository,IPhotoService photoService)
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

        var productViewModel = new IndexProductViewModel
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
                Image = result.Url.ToString(),
                AppUserId = productVM.AppUserId,
                Price = productVM.Price,
                Quantity = productVM.Quantity,
            };
            _productRepository.Add(product);
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "Photo upload failed");
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

        var product = new Product
        {
            ProductID = id,
            Name = productVM.Title,
            Description = productVM.Description,
            Image = photoResult.Url.ToString(),
            Category = productVM.Category,
            Quantity = productVM.Quantity,
            Price = productVM.Price,
        };

        _productRepository.Update(product);

        return RedirectToAction("Index");
    }

}
