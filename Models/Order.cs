using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iFood.Data.Enum;

namespace iFood.Models
{
	public class Order
	{
		[Key]
		public int Id { get; set; }
		[ForeignKey("AppUser")]
		public string AppUserId { get; set; }
		public DateTime OrderDate  { get; set; }
		public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
		public decimal TotalPrice => OrderDetails?.Sum(i => i.TotalPrice) ?? 0;
		[ForeignKey("MomoInfo")]
		public int? MomoInfoId { get; set;}
		public PaymentMethod? PaymentMethod {get; set;}
	}
}