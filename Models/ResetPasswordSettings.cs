namespace OnlineStoreAPI.Models
{
    public class ResetPasswordSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int TokenLifeTime { get; set; }
    }
}