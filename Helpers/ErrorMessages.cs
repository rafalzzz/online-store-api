namespace OnlineStoreAPI.Helpers
{
    public class ErrorMessages
    {
        public const string RegisterUserPasswordError = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, one digit, and one special character";
        public const string RegisterUserEmailError = "The account with the provided email address already exists";
        public const string LoginUserWrongEmailError = "Account with the provided email address doest not exist";
        public const string LoginUserWrongPasswordError = "Wrong password";
    }
}