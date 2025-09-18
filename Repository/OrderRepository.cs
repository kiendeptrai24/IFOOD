using iFood;
using iFood.Data;
using iFood.Data.Enum;
using iFood.Interfaces;
using iFood.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository.Interfaces;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderRepository(ApplicationDBContext context,IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool Add(Order order)
    {
        _context.Orders.Add(order);
        return Save();
    }
    public async Task<bool> AddAsync(Order order)
    {
        _context.Orders.AddAsync(order);
        return await SaveAsync();
    }

    public bool Delete(Order order)
    {
        _context.Orders.Remove(order);
        return Save();
    }

    public async Task<IEnumerable<Order>> GetAll()
    {
        
        return await _context.Orders.Include(i => i.OrderDetails).ToListAsync();
    }

    public async Task<List<Order>> GetAllByUserId()
    {
        var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
        var userOrders = _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Where(o => o.AppUserId == curUser)
            .OrderByDescending(o => o.OrderDate); // Sắp xếp giảm dần theo ngày đặt hàng

        return await userOrders.ToListAsync();
    }


    public async Task<Order> GetByIdAsync(int id)
    {
        return await _context.Orders
        .Include(o => o.OrderDetails) // Nạp thêm danh sách chi tiết đơn hàng
        .ThenInclude(od => od.Product) // Nạp thêm thông tin sản phẩm của từng chi tiết đơn hàng
        .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> GetByIdAsyncNoTracking(int id)
    {
         return await _context.Orders
        .AsNoTracking() // Không theo dõi thực thể => hiệu suất tốt hơn
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<int> GetCountByCategoryAsync(ProductCategory category)
    {
        return await _context.Products
            .Where(p => p.Category == category)
            .CountAsync();
    }

    public async Task<IEnumerable<Order>> GetProductsByCategoryAndSliceAsync(ProductCategory category, int offset, int size)
    {
            return await _context.Orders
            .Where(o => o.OrderDetails.Any(od => od.Product.Category == category))
            .Skip(offset)
            .Take(size)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetSliceAsync(int offset, int size)
    {
        return await _context.Orders
            .Include(i => i.OrderDetails)
            .OrderBy(o => o.status == Status.Completed) // false < true => chưa hoàn thành lên trước
            .ThenByDescending(o => o.OrderDate) // sắp xếp tăng dần theo OrderDate
            .Skip(offset)
            .Take(size)
            .ToListAsync();

    }

    public bool Save()
    {
        var saved = _context.SaveChanges();
        return saved > 0 ? true : false;
    }
    public async Task<bool> SaveAsync()
    {
        var saved = await _context.SaveChangesAsync();
        return saved > 0 ? true : false;
    }

    public async Task<IEnumerable<Order>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _context.Orders.ToListAsync(); // Trả về toàn bộ đơn hàng nếu không có từ khóa
        }

        return await _context.Orders
            .Where(o => 
                o.Ordercode.Contains(searchTerm) || // Tìm theo mã đơn hàng
                o.AppUserId.Contains(searchTerm) || // Tìm theo ID người mua
                o.status.ToString().Contains(searchTerm)) // Tìm theo trạng thái đơn hàng
            .Include(i => i.OrderDetails)
            .OrderBy(o => o.status == Status.Completed) // false < true => chưa hoàn thành lên trước
            .ThenBy(o => o.OrderDate)  // Sắp xếp theo ngày đặt hàng mới nhất
            .ToListAsync();
    }

    public bool Update(Order order)
    {
        _context.Orders.Update(order);
        return Save();
    }
}
