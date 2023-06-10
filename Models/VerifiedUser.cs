using OnlineStoreAPI.Enums;

namespace OnlineStoreAPI.Models
{
    public class VerifiedUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; }
    }
}