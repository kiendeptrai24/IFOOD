using iFood.Data.Enum;
using iFood.Models;

namespace iFood.ViewModels;

public class IndexDashboardViewModel
{
    public IEnumerable<Product> Products { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalClubs { get; set; }
    public int Category { get; set; }
    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}