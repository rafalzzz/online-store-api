namespace OnlineStoreAPI.Models
{
    public class ValidationError
    {
        public string Property { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
    }
}