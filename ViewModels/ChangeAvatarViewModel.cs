using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace iFood.ViewModels
{
    public class ChangeAvatarViewModel
    {
        [Required(ErrorMessage = "Please select an image.")]
        public IFormFile? Avatar { get; set; }
    }
}
