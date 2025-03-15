using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using iFood.Models;
using iFood.Data;
using iFood.Interfaces;
using iFood.ViewModels;
using iFood.Data.Enum;

namespace iFood.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductRepository _productRepository;

    public HomeController(ILogger<HomeController> logger,IProductRepository productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Index(string searchTerm,int category = -1, int page = 1, int pageSize = 8)
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
            return RedirectToAction("Index","Product",searchTerm);
            // products = await _productRepository.SearchAsync(searchTerm);
        }
        var count = category switch
        {
            -1 => await _productRepository.GetCountAsync(),
            _ => await _productRepository.GetCountByCategoryAsync((ProductCategory)category),
        };

        var homeViewModel = new HomeViewModel
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalProducts = count,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Category = category,
        };
        return View(homeViewModel);
    }

    public async Task<IActionResult> Privacy()
    {
        var products = await _productRepository.GetAll();
        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}
