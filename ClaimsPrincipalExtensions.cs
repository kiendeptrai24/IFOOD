using System.Security.Claims;
using iFood.Models;
using Microsoft.AspNetCore.Identity;

namespace iFood
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public static async Task<AppUser> GetUserById(this ClaimsPrincipal user,UserManager<AppUser> userManager)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await userManager.FindByIdAsync(userId);
        }
    }
}