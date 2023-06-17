namespace OnlineStoreAPI.Models
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int TokenLifeTime { get; set; }
        public int RefreshTokenLifeTime { get; set; }
    }
}