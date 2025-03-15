using iFood.Models;
using iFood.Models.Momo;

namespace iFood.Interfaces;

public interface IMomoRepository
{
    Task<IEnumerable<MomoInfo>> GetAll();
    Task<List<MomoInfo>> GetAllByUserId();
    Task<MomoInfo> GetByIdAsync(int id);
    Task<IEnumerable<MomoInfo>> GetSliceAsync(int offset, int size);
    Task<int> GetCountAsync();
    Task<MomoInfo> GetByIdAsyncNoTracking(int id);
    bool Add(MomoInfo momoInfo);
    Task<bool> AddAsync(MomoInfo momoInfo);
    bool Update(MomoInfo momoInfo);
    bool Delete(MomoInfo momoInfo);
    bool Save();
    Task<bool> SaveAsync();
}
