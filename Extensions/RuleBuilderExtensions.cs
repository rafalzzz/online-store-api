using FluentValidation;

namespace OnlineStoreAPI.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static void FirstName<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 2, int maxLength = 150)
        {
            ruleBuilder
            .NotEmpty()
            .WithMessage("First name is required")
            .MinimumLength(minimumLength)
            .WithMessage($"First name must contain at least {minimumLength} characters")
            .MaximumLength(maxLength)
            .WithMessage($"First name can contain up to {maxLength} characters")
            .Matches("^[a-zA-Z]")
            .WithMessage("First name can only contain letters");
        }

        public static void LastName<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 2, int maxLength = 150)
        {
            ruleBuilder
            .NotEmpty()
            .WithMessage("Last name is required")
            .MinimumLength(minimumLength)
            .WithMessage($"Last name must contain at least {minimumLength} characters")
            .MaximumLength(maxLength)
            .WithMessage($"Last name can contain up to {maxLength} characters")
            .Matches("^[a-zA-Z]")
            .WithMessage("Last name can only contain letters");
        }

        public static void Email<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address");
        }

        public static void Password<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 8)
        {
            ruleBuilder
            .MinimumLength(minimumLength)
            .WithMessage($"Password must contain at least {minimumLength} characters")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character");
        }

        public static void Token<T>(this IRuleBuilder<T, string> ruleBuilder, int minimumLength = 20)
        {
            ruleBuilder
            .MinimumLength(minimumLength)
            .WithMessage($"Token must contain at least {minimumLength} characters");
        }
    }
}