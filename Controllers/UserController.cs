using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;

namespace OnlineStoreAPI.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public UserController(IUserService userService, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator)
        {
            _userService = userService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        private IEnumerable<ValidationError> GetValidationErrorsResult(ValidationResult validationResult)
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

        [HttpPost]
        public ActionResult Register([FromBody] RegisterRequest registerUserDto)
        {
            var registerRequestValidation = _registerValidator.Validate(registerUserDto);
            var validationResultErrors = GetValidationErrorsResult(registerRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            var id = _userService.CreateUser(registerUserDto);

            if (id is null)
            {
                return BadRequest("User with the provided email address already exists");
            }

            return Created($"/api/users/{id}", null);
        }

        [HttpPost("login")]
        public ActionResult LoginRequest([FromBody] LoginRequest loginUserDto)
        {
            var loginRequestValidation = _loginValidator.Validate(loginUserDto);
            var validationResultErrors = GetValidationErrorsResult(loginRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            bool? userIsVerified = _userService.VerifyUser(loginUserDto);

            return userIsVerified switch
            {
                null => NotFound("Account with the provided email address doest not exist"),
                false => BadRequest("Incorrect password"),
                true => Ok(),
            };
        }
    }
}