using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iFood.Models
{
	public class Cart
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[ForeignKey("AppUser")]
        public string AppUserId { get; set; }
		public AppUser? AppUser { get; set; }
		[Required]
		[ForeignKey("Product")]
		public int ProductId { get; set; }
		public Product? product { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime ClosestUdateDate { get; set; } = DateTime.UtcNow;

		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public decimal Total => Quantity * Price;
	}
}