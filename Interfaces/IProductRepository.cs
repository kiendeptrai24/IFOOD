using iFood.Data.Enum;
using iFood.Models;

namespace iFood.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAll();
    Task<Product> GetByIdAsync(int id);
    void UnTracking();
    Task<Product> GetByIdNoTrackingAsync(int id);

    Task<IEnumerable<Product>> GetSliceAsync(int offset, int size);
    Task<IEnumerable<Product>> GetProductsByCategoryAndSliceAsync(ProductCategory category, int offset, int size);
    Task<int> GetCountAsync();
    Task<IEnumerable<Product>> GetBestSellerProductsAsync();

    Task<int> GetCountByCategoryAsync(ProductCategory category);
    Task<Product> GetByIdAsyncNoTracking(int id);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    
    void CheckAttackProduct(Product product);
    bool Add(Product product);
    bool Update(Product product);
    bool Delete(Product product);

    bool Save();
}
