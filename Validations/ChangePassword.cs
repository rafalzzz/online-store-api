using FluentValidation;
using OnlineStoreAPI.Extensions;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Validations
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(requestBody => requestBody.Password)
            .Cascade(CascadeMode.Stop)
            .Password();
        }
    }
}