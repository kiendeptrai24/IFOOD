using iFood.Data.Enum;

namespace iFood.ViewModels;

public class CreateProductViewModel
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? SoldOut { get; set; }
     public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime Date {get; set; } = DateTime.Now;
    public string? URL { get; set;}
    public IFormFile? Image { get; set; }
    public ProductCategory? Category { get; set; }
    public string? AppUserId { get; set; }
}