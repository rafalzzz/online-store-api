using System.ComponentModel.DataAnnotations;
using OnlineStoreAPI.Helpers;

namespace OnlineStoreAPI.Models
{
    public class LoginUserDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;
        [Required]
        [RegularExpression(RegexPatterns.Password, ErrorMessage = ErrorMessages.RegisterUserPasswordError)]
        public string Password { get; set; } = null!;
    }
}