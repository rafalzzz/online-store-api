using FluentValidation;
using OnlineStoreAPI.Extensions;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Validations
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(requestBody => requestBody.FirstName)
            .Cascade(CascadeMode.Stop)
            .FirstName();

            RuleFor(requestBody => requestBody.LastName)
            .Cascade(CascadeMode.Stop)
            .LastName();

            RuleFor(requestBody => requestBody.Email)
            .Cascade(CascadeMode.Stop)
            .Email();

            RuleFor(requestBody => requestBody.Password)
            .Cascade(CascadeMode.Stop)
            .Password();
        }
    }
}