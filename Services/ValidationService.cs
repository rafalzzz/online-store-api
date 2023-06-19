
using FluentValidation.Results;
using OnlineStoreAPI.Models;

namespace OnlineStoreAPI.Services
{
    public interface IRequestValidationService
    {
        IEnumerable<ValidationError> GetValidationErrorsResult(ValidationResult validationResult);
    }

    public class RequestValidationService : IRequestValidationService
    {
        public RequestValidationService()
        {
        }

        public IEnumerable<ValidationError> GetValidationErrorsResult(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                var errorList = validationResult.Errors
                .Select(error => new ValidationError
                {
                    Property = error.PropertyName,
                    ErrorMessage = error.ErrorMessage
                });

                return errorList;
            }

            return null;
        }
    }
}