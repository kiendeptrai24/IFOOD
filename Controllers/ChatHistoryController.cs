using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using iFood.Data;
using Microsoft.EntityFrameworkCore;
using iFood.Models;
using System.Text.RegularExpressions;
using System.Security.Claims;
using iFood;

public class ChatHistoryController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        string userId = "chatbot";
        if(User.Identity.IsAuthenticated)
            userId = User?.GetUserId();
        var chat = HttpContext.Session.GetObjectFromJson<List<ChatMessage>>(userId) 
                   ?? new List<ChatMessage>();
        return Json(chat);
    }
}
