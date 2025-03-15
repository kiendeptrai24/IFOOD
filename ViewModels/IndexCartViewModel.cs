using iFood.Data.Enum;
using iFood.Models;

namespace iFood.ViewModels;

public class IndexCartViewModel
{
    public List<Cart>? CartItems { get; set;}
    public int? CartCount  => CartItems?.Count ?? 0;
    public decimal? TotalPrice => CartItems?.Sum(i => i.Total) ?? 0;
}