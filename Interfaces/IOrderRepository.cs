using iFood.Data.Enum;
using iFood.Models;
using iFood.Models.Momo;

namespace iFood.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAll();
    Task<List<Order>> GetAllByUserId();
    Task<Order> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetSliceAsync(int offset, int size);
    Task<int> GetCountByCategoryAsync(ProductCategory category);
    Task<IEnumerable<Order>> GetProductsByCategoryAndSliceAsync(ProductCategory category, int offset, int size);
    Task<IEnumerable<Order>> SearchAsync(string searchTerm);
    Task<int> GetCountAsync();
    Task<Order> GetByIdAsyncNoTracking(int id);
    bool Add(Order order);
    Task<bool> AddAsync(Order order);
    bool Update(Order order);
    bool Delete(Order order);
    bool Save();
    Task<bool> SaveAsync();
}
