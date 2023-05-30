using OnlineStoreAPI.Enums;

namespace OnlineStoreAPI.Models
{
    public class VerifiedUser
    {
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
    }
}