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
        var userOrders = _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Product) 
        .Where(r => r.AppUserId == curUser);
        return await userOrders.ToListAsync();
    }

    public Task<Order> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Order> GetByIdAsyncNoTracking(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public Task<IEnumerable<Order>> GetSliceAsync(int offset, int size)
    {
        throw new NotImplementedException();
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

    public bool Update(Order order)
    {
        _context.Orders.Update(order);
        return Save();
    }
}
