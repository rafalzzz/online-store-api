namespace OnlineStoreAPI.Requests
{
    public class ChangePasswordRequest
    {
        public string Token { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}