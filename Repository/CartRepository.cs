using iFood;
using iFood.Data;
using iFood.Data.Enum;
using iFood.Interfaces;
using iFood.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository.Interfaces;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CartRepository(ApplicationDBContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public bool Add(Cart cart)
    {
        _context.Carts.Add(cart);
        return Save();
    }

    public bool Delete(Cart cart)
    {
        var existingUser = _context.Users.Local.FirstOrDefault(u => u.Id == cart.AppUserId);
        if (existingUser != null)
        {
            _context.Entry(existingUser).State = EntityState.Detached; // Ngắt theo dõi thực thể cũ
        }

        var existingProduct = _context.Products.Local
            .FirstOrDefault(p => p.ProductID == cart.ProductId);
        if (existingProduct != null)
        {
            _context.Entry(existingProduct).State = EntityState.Detached;
        }
        _context.Carts.Remove(cart);
        return Save();
    }
    public async Task<Cart> GetCartByProductIdOfIdentificationUserAsync(int id)
    {
        var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();

        return await _context.Carts.Include(i => i.AppUser).Where(r => r.AppUserId == curUser)
        .FirstOrDefaultAsync(c => c.ProductId == id);
    }
    

    public async Task<IEnumerable<Cart>> GetAll()
    {
        return await _context.Carts.Include(i => i.AppUser).Include(i => i.product).ToListAsync();
    }

    public async Task<Cart> GetByIdAsync(int id)
    {
        return await _context.Carts.Include(i => i.product).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cart> GetByIdAsyncNoTracking(int id)
    {
        return await _context.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<IEnumerable<Cart>> GetCartsByCategoryAndSliceAsync(ProductCategory category, int offset, int size)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Carts.CountAsync();
    }

    public Task<int> GetCountByCategoryAsync(ProductCategory category)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Cart>> GetSliceAsync(int offset, int size)
    {
        throw new NotImplementedException();
    }

    public bool Save()
    {
        var saved = _context.SaveChanges();
        return saved > 0 ? true : false;
    }

    public bool Update(Cart cart)
    {
        _context.Carts.Update(cart);
        return Save();
    }

    public async Task<List<Cart>> GetAllByUserId()
    {
        var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
        var userCarts = _context.Carts.Include(i =>i.product).Where(r => r.AppUser.Id == curUser);
        return userCarts.ToList();
    }

    public bool DeleteAll(List<Cart> carts)
    {
        foreach (var cart in carts)
        {
            _context.Entry(cart).State = EntityState.Detached; // Ngăn không tracking cart
        }
        _context.Carts.RemoveRange(carts);
        _context.SaveChanges();
        return Save();
    }

    public Cart GetById(int id)
    {
        return _context.Carts.Include(i => i.product).FirstOrDefault(c => c.Id == id);
    }

    public async Task<List<Cart>> GetCartsByIdNoTracking(List<int> ids)
    {
        var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
    
        if (curUser == null)
        {
            return new List<Cart>(); // Trả về danh sách rỗng nếu không có user
        }

        var userCarts = await _context.Carts
            .Include(i => i.AppUser)
            .Include(i => i.product)
            .Where(r => r.AppUser.Id == curUser && ids.Contains(r.Id))
            .AsNoTracking() // Lọc theo danh sách ID
            .ToListAsync(); // Chuyển IQueryable thành List<Cart>

        return userCarts;
    }
}
