using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iFood.Models;

namespace iFood.Models
{
	public class ChatMessage
	{
		public string Sender { get; set; } // "user" hoặc "bot"
		public string Message { get; set; }
		public List<object> Products { get; set; } = new List<object>(); // Để tương thích với object ẩn danh
		public DateTime Timestamp { get; set; }
	}


}