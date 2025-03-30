using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using iFood.Data;
using Microsoft.EntityFrameworkCore;
using iFood.Models;

namespace iFood.Controllers          // Định nghĩa namespace cho Controller.
{
    public class ChatbotController : Controller  // Định nghĩa controller tên ChatbotController.
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly ApplicationDBContext _dbContext;


        public ChatbotController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApplicationDBContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient();       // Tạo HttpClient từ factory.
            _geminiApiKey = configuration.GetValue<string>("GeminiApiKey");  // Đọc API Key từ cấu hình (có vẻ code này đang làm sai, đáng lẽ nên truyền vào key string từ appsettings).
            _dbContext = dbContext;
        }
      [HttpPost]
public async Task<IActionResult> GetResponse(string message)
{
    if (string.IsNullOrEmpty(message))
    {
        return Json(new { response = "Vui lòng nhập câu hỏi!" });
    }

    // Tách từ khóa tìm kiếm
    var keywords = message.ToLower().Split(' ');

    // Lọc sản phẩm theo từ khóa
    var relevantProducts = await _dbContext.Products
        .Where(p => keywords.Any(k => p.Name.ToLower().Contains(k) || p.Description.ToLower().Contains(k)))
        .Select(p => new 
        { 
            p.ProductID, 
            p.Name, 
            p.Quantity,
            p.Price, 
            p.Description, 
            p.SoldOut,
            p.Image
        })
        .ToListAsync();

    // Nếu không tìm thấy sản phẩm phù hợp, lấy tất cả sản phẩm
    if (!relevantProducts.Any())
    {
        relevantProducts = await _dbContext.Products
            .Select(p => new { p.ProductID, p.Name,p.Quantity, p.Price, p.Description, p.SoldOut, p.Image })
            .ToListAsync();
    }

    // Chuẩn bị danh sách sản phẩm dưới dạng JSON
    var productsResponse = relevantProducts.Select(p => new
    {
        id = p.ProductID,
        name = p.Name,
        price = $"{p.Price} VND",
        description = p.Description,
        soldout = p.SoldOut,
        qualiti = p.Quantity ,
        img = p.Image,
    }).ToList();
    string productContext = "Danh sách sản phẩm:\n";
    foreach (var product in productsResponse)
    {
        productContext += $"- {product.name} (ID: {product}): Giá {product.price} USDUSD, {product.description}, {(product.qualiti > 0 ? "còn hàng" : "hết hàng")}\n";
    }

                // ---- Bước 3: Tạo prompt cho Gemini AI ----
            string fullMessage = $"  Bạn là chatbot bán hàng. Dữ liệu sản phẩm:\n{productContext}\nHãy trả lời câu hỏi: '{message}' bằng tiếng Việt một cách chính xác và dễ hiểu." ;

            // ---- Bước 4: Chuẩn bị payload gửi tới Gemini ----
            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = fullMessage } } } }  // Định dạng JSON yêu cầu của Gemini.
            };

    // 🔴 Thay đổi: Yêu cầu Gemini trả về JSON, không trả về Markdown/text.
    // var payload = new
    // {
    //     contents = new[]
    //     {
    //         new
    //         {
    //             parts = new[]
    //             {
    //                 new
    //                 {
    //                     text = "Hãy trả về danh sách JSON không Markdown, không có dấu *, không có ký tự thừa:\n" +
    //                            JsonConvert.SerializeObject(productsResponse)
    //                 }
    //             }
    //         }
    //     }
    // };

    var jsonPayload = JsonConvert.SerializeObject(payload);
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";

    try
    {
        var response = await _httpClient.PostAsync(requestUrl, content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();

        dynamic result = JsonConvert.DeserializeObject(responseString);
        string botResponse = "Xin lỗi, tôi không thể xử lý yêu cầu của bạn ngay lúc này.";

        // 🔴 Kiểm tra dữ liệu trước khi truy xuất
        if (result?.candidates != null && result.candidates.Count > 0 &&
            result.candidates[0]?.content?.parts != null && result.candidates[0].content.parts.Count > 0)
        {
            botResponse = result.candidates[0].content.parts[0].text;
        }
        

        // Trả về JSON chứa phản hồi và danh sách sản phẩm
        return Json(new { response = botResponse, products = productsResponse });
    }
    catch
    {
        return Json(new { response = "Xin lỗi, tôi gặp sự cố khi xử lý yêu cầu. Vui lòng thử lại!" });
    }
}

    }
}