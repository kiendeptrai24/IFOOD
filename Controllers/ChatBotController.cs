using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using iFood.Data;
using System.IO;
using System;

namespace layout.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly ApplicationDBContext _context;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApplicationDBContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = configuration.GetValue<string>("GeminiApiKey");
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GetResponse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Json(new { response = "Vui lòng nhập câu hỏi!", imageUrl = (string)null });
            }

            var keywords = message.ToLower().Split(' ');
            var relevantProducts = await _context.Products
                .Where(p => keywords.Any(k => p.Name.ToLower().Contains(k) || p.Description.ToLower().Contains(k)))
                .Select(p => new { p.Name, p.Price, p.Description, p.Quantity, p.Image })
                .ToListAsync();

            if (!relevantProducts.Any())
            {
                relevantProducts = await _context.Products
                    .Select(p => new { p.Name, p.Price, p.Description, p.Quantity, p.Image })
                    .ToListAsync();
            }

            string productContext = "Danh sách sản phẩm:\n";
            string imageBase64 = null;

            foreach (var product in relevantProducts)
            {
                productContext += $"- {product.Name}: {product.Price * 24000} VND, {product.Description}, {(product.Quantity > 0 ? "còn hàng" : "hết hàng")}\n";

                if (!string.IsNullOrEmpty(product.Image) && imageBase64 == null)
                {
                    string imageUrl = product.Image;
                    
                    using (HttpClient httpClient = new HttpClient())
                    {
                        try
                        {
                            byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                            imageBase64 = Convert.ToBase64String(imageBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Lỗi khi tải ảnh: " + ex.Message);
                            imageBase64 = null; // Không sử dụng ảnh nếu có lỗi
                        }
                    }
                }
            }

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = $"Bạn là chatbot bán hàng. Dữ liệu sản phẩm:\n{productContext}\n Câu hỏi: '{message}'." },
                            !string.IsNullOrEmpty(imageBase64) 
                                ? new { inlineData = new { mimeType = "image/jpeg", data = imageBase64 } }
                                : null
                        }
                        .Where(p => p != null) // Lọc null nếu không có ảnh
                        .ToArray()
                    }
                }
            };


            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var requestUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-pro:generateContent?key={_geminiApiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { response = "Lỗi API!", error = responseString });
                }

                dynamic result = JsonConvert.DeserializeObject(responseString);
                string botResponse = result.candidates[0].content.parts[0].text;

                return Json(new { response = botResponse, imageUrl = imageBase64 });
            }
            catch (Exception)
            {
                return Json(new { response = "Xin lỗi, tôi gặp sự cố khi xử lý yêu cầu!", imageUrl = (string)null });
            }
        }
    }
}
