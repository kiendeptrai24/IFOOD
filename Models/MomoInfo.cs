using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iFood.Models;

namespace iFood.Models
{
	public class MomoInfo
	{
		[Key]
		public int Id { get; set; }
       	public string OrderInfo {get; set;}
       	public string FullName {get; set;}
		public string MomoTransactionId {get; set;}
       	public decimal Price {get; set;}
       	public DateTime DatePaid {get; set;}
		[ForeignKey("AppUser")]
		public string? AppUserId {get; set;}
		[ForeignKey("Order")]
		public int? OrderId { get; set; } 
		public Order? Order {get; set;}

	}
}