using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using iFood.Data;
using Microsoft.EntityFrameworkCore;
using iFood.Models;
using System.Text.RegularExpressions;


namespace iFood.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly ApplicationDBContext _dbContext;

        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApplicationDBContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _geminiApiKey = configuration.GetValue<string>("GeminiApiKey");
            _dbContext = dbContext;
        }
        [HttpPost]
        public async Task<IActionResult> GetResponse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return Json(new { response = "Please enter your question!" });
            }

            // Extract keywords from the message
            var keywords = message.ToLower().Split(' ');
            var relevantProducts = await _dbContext.Products
                .Where(p => keywords.Any(k =>
                    p.Name.ToLower().Contains(k) ||
                    p.Category.ToString().ToLower().Contains(k) ||
                    p.Price.ToString().ToLower().Contains(k) ||
                    p.Quantity.ToString().ToLower().Contains(k) ||
                    p.Status.ToString().ToLower().Contains(k) 
                )) 
                .Select(p => new
                {
                    p.ProductID,
                    p.Name,
                    p.Quantity,
                    p.Category,
                    p.Price,
                    p.Image,
                    p.Status
                })
                .Take(3)
                .ToListAsync(); 
            // If no relevant product found, return default top 4 products
            if (!relevantProducts.Any())
            {
                relevantProducts = await _dbContext.Products
                    .Select(p => new { p.ProductID, p.Name, p.Quantity, p.Category, p.Price, p.Image, p.Status })
                    .ToListAsync();
            }

            // Prepare product list in JSON
            var productsResponse = relevantProducts.Select(p => new
            {
                id = p.ProductID,
                name = p.Name,
                price = $"{p.Price}",
                category = p.Category,
                qualiti = p.Quantity,
                img = p.Image,
                status = p.Status,
            }).ToList();


            // Format product list as plain text
            StringBuilder productContext = new StringBuilder("Here are some products you might be interested in:\r\n\r\n");
            foreach (var product in relevantProducts)
            {
                productContext.AppendLine($" **Product Name**: {product.Name}\r\n");
                productContext.AppendLine($" **Product ID**: {product.ProductID}\r\n");
                productContext.AppendLine($" **Price**: {product.Price} VND\r\n");
                productContext.AppendLine($" **Category**: {product.Category}\r\n");
                productContext.AppendLine($" **Availability**: {(product.Quantity > 0 ? "In stock" : "Out of stock")}\r\n");
                productContext.AppendLine($" **Status**: {product.Status}\r\n");
                productContext.AppendLine("------------------------------------------------\r\n");
            }

            // Compose full message for Gemini with instruction to avoid using Markdown
            string fullMessage = "You are a sales chatbot. Provide a clear response and do not use Markdown. Below is the product information:\r\n\r\n"
                                 + productContext.ToString()
                                 + $"(If the question is in a language, answer in that language.)"
                                 + $"Please answer the question or suggest relevant products: '{message}'. Ensure each product detail is separated correctly by line breaks.";

            // Prepare payload for Gemini API
            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = fullMessage } } } }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(responseString);
                string botResponse = "Sorry, I couldn't process your request at the moment.";

                // Parse response from Gemini
                if (result?.candidates != null && result.candidates.Count > 0 &&
                    result.candidates[0]?.content?.parts != null && result.candidates[0].content.parts.Count > 0)
                {
                    botResponse = result.candidates[0].content.parts[0].text;
                }
                List<int> productIds = new List<int>();
                if (!string.IsNullOrEmpty(botResponse))
                {
                    var matches = Regex.Matches(botResponse, @"Product ID:\s*(\d+)");
                    foreach (Match match in matches)
                    {
                        if (int.TryParse(match.Groups[1].Value, out int id))
                        {
                            productIds.Add(id);
                        }
                    }
                }

                // B2: Truy vấn sản phẩm từ cơ sở dữ liệu theo danh sách ID
                var matchedProducts = new List<Product>();
                if (productIds.Any())
                {
                    matchedProducts = await _dbContext.Products
                        .Where(p => productIds.Contains(p.ProductID))
                        .ToListAsync();
                }

                // B3: Chuẩn hóa dữ liệu sản phẩm trả về cho phía client
                var resultProductsResponse = matchedProducts.Select(p => new
                {
                    id = p.ProductID,
                    name = p.Name,
                    price = $"{p.Price}",
                    category = p.Category,
                    qualiti = p.Quantity,
                    img = p.Image,
                    status = p.Status
                }).ToList();
                var history = HttpContext.Session.GetObjectFromJson<List<ChatMessage>>("ChatHistory") ?? new List<ChatMessage>();

                // Add user message
                history.Add(new ChatMessage
                {
                    Sender = "user",
                    Message = message,
                    Timestamp = DateTime.Now
                });

                // Add bot response
                history.Add(new ChatMessage
                {
                    Sender = "bot",
                    Message = botResponse,
                    Timestamp = DateTime.Now
                });
                string userId = "chatbot";
                if(User.Identity.IsAuthenticated)
                {
                    userId = User?.GetUserId();
                }
                HttpContext.Session.SetObjectAsJson(userId, history);

                // Return JSON with bot response and product list
                return Json(new { response = botResponse, products = resultProductsResponse });
            }
            catch
            {
                return Json(new { response = "Sorry, there was an error processing your request. Please try again!" });
            }
        }
    }
}
