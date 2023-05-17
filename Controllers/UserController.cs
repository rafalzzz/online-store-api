using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IJwtService _jwtService;

        public UserController(IUserService userService, IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator, IJwtService jwtService)
        {
            _userService = userService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _jwtService = jwtService;
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

            object userEmail = _userService.VerifyUser(loginUserDto);

            switch (userEmail)
            {
                case VerifyUserError.EmailNoExist:
                    return NotFound("Account with the provided email address doest not exist");
                case VerifyUserError.WrongPassword:
                    return BadRequest("Incorrect password");
                default:
                    string token = _jwtService.GenerateToken((string)userEmail);
                    CookieOptions cookieOptions = _jwtService.GetCookieOptions();
                    Response.Cookies.Append(CookieNames.AccessToken, token, cookieOptions);
                    return Ok();
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            CookieOptions cookieOptions = _jwtService.RemoveAccessTokenCookieOptions();
            Response.Cookies.Append(CookieNames.AccessToken, string.Empty, cookieOptions);
            return Ok();
        }

        [HttpGet("user-data")]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        public ActionResult GetUserData()
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                return NotFound();
            }

            var userEmail = emailClaim.Value;
            return Ok(emailClaim.Value);
        }
    }
}