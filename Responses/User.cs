using OnlineStoreAPI.Enums;

namespace OnlineStoreAPI.Responses
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
    }
}