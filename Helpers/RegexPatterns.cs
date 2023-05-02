namespace OnlineStoreAPI.Helpers
{
    public class RegexPatterns
    {
        public const string OnlyLetters = @"^[a-zA-Z''-'\s]+$";
        public const string Password = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$";
    }
}