using FluentValidation;
using OnlineStoreAPI.Extensions;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Validations
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
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

            RuleFor(requestBody => requestBody.Role)
            .Cascade(CascadeMode.Stop)
            .Role();

        }
    }
}