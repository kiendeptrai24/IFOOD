using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iFood.Data.Enum;

namespace iFood.Models
{
	public class OrderDetail
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("Product")]
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public int Quantity { get; set; }
		public decimal TotalPrice => Product != null ? Product.Price * Quantity : 0;

	}
}