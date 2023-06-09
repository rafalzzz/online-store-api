using OnlineStoreAPI.Enums;

namespace OnlineStoreAPI.Requests
{
    public class UserRequestDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
    }
}