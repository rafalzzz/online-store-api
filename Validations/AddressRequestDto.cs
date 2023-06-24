using FluentValidation;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Validations
{
    public class AddressRequestDtoValidator : AbstractValidator<AddressRequestDto>
    {
        public AddressRequestDtoValidator()
        {
            RuleFor(requestBody => requestBody.AddressName)
            .NotEmpty().WithMessage("AddressName is required.")
            .Length(1, 50).WithMessage("AddressName must be between 1 and 50 characters long.");

            RuleFor(requestBody => requestBody.Country)
                .NotEmpty().WithMessage("Country is required.")
                .Length(1, 50).WithMessage("Country must be between 1 and 50 characters long.");

            RuleFor(requestBody => requestBody.City)
                .NotEmpty().WithMessage("City is required.")
                .Length(1, 50).WithMessage("City must be between 1 and 50 characters long.");

            RuleFor(requestBody => requestBody.Address)
                .NotEmpty().WithMessage("Address is required.")
                .Length(1, 100).WithMessage("Address must be between 1 and 100 characters long.");

            RuleFor(requestBody => requestBody.PostalCode)
                .NotEmpty().WithMessage("PostalCode is required.")
                .Matches(@"^\d{2}-\d{3}$").WithMessage("PostalCode must be in the 'XX-XXX' format.");

            RuleFor(requestBody => requestBody.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required.")
                .Matches(@"^\+\d{2}\s\d{3}\s\d{3}\s\d{3}$").WithMessage("PhoneNumber must be in the '+XX XXX XXX XXX' format.");
        }
    }
}