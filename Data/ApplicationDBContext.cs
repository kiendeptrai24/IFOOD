using iFood.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace iFood.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<Address> Addresses { get; set;}
        public DbSet<Product> Products { get; set;}
        public DbSet<Order> Orders { get; set;}
        public DbSet<Review> Reviews { get; set;}
        public DbSet<Cart> Carts{ get; set;}
        public DbSet<Wishlist> Wishlists { get; set;}
        public DbSet<MomoInfo> MomoInfos { get; set;}
        public DbSet<OrderDetail> OrderDetails { get; set;}
    }
}
