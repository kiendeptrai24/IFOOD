using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace iFood.ViewModels;
public class ProfileUserViewModel
{
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Name { get; set; }

    public IFormFile? Avatar { get; set; }
    public string? AvatarUrl { get; set; }


    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Phone]
    public string? Phone { get; set; }

}