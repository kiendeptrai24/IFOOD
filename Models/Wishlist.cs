using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iFood.Models;

namespace iFood.Models
{
	public class Wishlist
	{
		[Key]
		public int Id { get; set; }
        [ForeignKey("AppUser")]
		public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

		[ForeignKey("Product")]
		public int ProductId { get; set; }
		public Product? Product { get; set; }
	}
}