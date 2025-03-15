using iFood;
using iFood.Data;
using iFood.Data.Enum;
using iFood.Interfaces;
using iFood.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository.Interfaces;

public class MomoRepository : IMomoRepository
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public MomoRepository(ApplicationDBContext context,IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool Add(MomoInfo momoInfo)
    {
        _context.MomoInfos.Add(momoInfo);
        return Save();
    }
    public async Task<bool> AddAsync(MomoInfo momoInfo)
    {
        _context.MomoInfos.AddAsync(momoInfo);
        return await SaveAsync();
    }

    public bool Delete(MomoInfo momoInfo)
    {
        _context.MomoInfos.Remove(momoInfo);
        return Save();
    }

    public async Task<IEnumerable<MomoInfo>> GetAll()
    {
        return await _context.MomoInfos.Include(i => i.Order).ToListAsync();
    }

    public async Task<List<MomoInfo>> GetAllByUserId()
    {
        var curUser = _httpContextAccessor.HttpContext?.User.GetUserId();
        var userOrders = _context.MomoInfos.Include(i => i.Order).Where(r => r.AppUserId == curUser);
        return await userOrders.ToListAsync();
    }

    public Task<MomoInfo> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<MomoInfo> GetByIdAsyncNoTracking(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetCountAsync()
    {
        return _context.MomoInfos.Count();
    }

    public Task<IEnumerable<MomoInfo>> GetSliceAsync(int offset, int size)
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

    public bool Update(MomoInfo momoInfo)
    {
        throw new NotImplementedException();
    }
    
}
