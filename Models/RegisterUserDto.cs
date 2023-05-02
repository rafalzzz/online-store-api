using System.ComponentModel.DataAnnotations;
using OnlineStoreAPI.Helpers;

namespace OnlineStoreAPI.Models
{
    public class RegisterUserDto
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression(RegexPatterns.OnlyLetters)]
        public string FirstName { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        [RegularExpression(RegexPatterns.OnlyLetters)]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;
        [Required]
        [RegularExpression(RegexPatterns.Password, ErrorMessage = ErrorMessages.RegisterUserPasswordError)]
        public string Password { get; set; } = null!;
    }
}