using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace iFood.Models
{
    public class AppUser : IdentityUser
    {
        public int? Pace { get; set; }
        public string? ProfileImageUrl { get; set; }
        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public Address? Address { get; set; }

        public ICollection<Product>? Products { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Wishlist>? Wishlists { get; set; }
    }
}