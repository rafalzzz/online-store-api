using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Controllers
{
    [Route(ControllerRoutes.UserController)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IResetPasswordTokenService _resetPasswordTokenService;
        private readonly IRequestValidationService _requestValidationService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IValidator<UpdateUserRequest> _updateUserValidator;

        public UserController(
            IUserService userService,
            IValidator<RegisterRequest> registerValidator,
            IAccessTokenService accessTokenService,
            IRefreshTokenService refreshTokenService,
            IResetPasswordTokenService resetPasswordTokenService,
            IRequestValidationService requestValidationService,
            IValidator<LoginRequest> loginValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IValidator<UpdateUserRequest> updateUserValidator
            )
        {
            _userService = userService;
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
            _resetPasswordTokenService = resetPasswordTokenService;
            _requestValidationService = requestValidationService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _changePasswordValidator = changePasswordValidator;
            _updateUserValidator = updateUserValidator;
        }

        [HttpPost]
        public ActionResult Register([FromBody] RegisterRequest registerUserDto)
        {
            var registerRequestValidation = _registerValidator.Validate(registerUserDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(registerRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            var id = _userService.CreateUser(registerUserDto);

            if (id is null)
            {
                return BadRequest("User with the provided email address already exists");
            }

            return Ok();
        }

        [HttpPost(UserControllerEndpoints.Login)]
        public ActionResult Login([FromBody] LoginRequest loginUserDto)
        {
            var loginRequestValidation = _loginValidator.Validate(loginUserDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(loginRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            (VerifyUserError error, User user, bool isError) verifiedUser = _userService.VerifyUser(loginUserDto);

            switch (verifiedUser.error)
            {
                case VerifyUserError.EmailNoExist:
                    return NotFound("Account with the provided email address doest not exist");
                case VerifyUserError.WrongPassword:
                    return BadRequest("Incorrect password");
                case VerifyUserError.WrongRole:
                    return StatusCode(500, "User role error");
                default:
                    string userId = verifiedUser.user.Id.ToString();
                    string role = _userService.GetUserRoleDescription(verifiedUser.user.Role);
                    string token = _accessTokenService.GenerateAccessToken(userId, role);
                    CookieOptions accessTokenCookieOptions = _accessTokenService.GetAccessTokenCookieOptions();
                    Response.Cookies.Append(CookieNames.AccessToken, token, accessTokenCookieOptions);

                    string refreshToken = _refreshTokenService.GenerateRefreshToken(userId);
                    _userService.SaveUserRefreshToken(refreshToken, verifiedUser.user);
                    CookieOptions refreshTokenCookieOptions = _refreshTokenService.GetRefreshTokenCookieOptions();
                    Response.Cookies.Append(CookieNames.RefreshToken, refreshToken, refreshTokenCookieOptions);

                    return Ok();
            }
        }

        [HttpPost(UserControllerEndpoints.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordDto)
        {
            var resetPasswordRequestValidation = _resetPasswordValidator.Validate(resetPasswordDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(resetPasswordRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            bool emailExist = _userService.CheckIfEmailExist(resetPasswordDto.Email);

            if (!emailExist)
            {
                return BadRequest("Account with the provided email address doest not exist");
            }

            await _resetPasswordTokenService.SendResetPasswordToken(resetPasswordDto.Email);

            return Ok();
        }

        [HttpPut(UserControllerEndpoints.ChangePassword)]
        public ActionResult ChangePassword([FromBody] ChangePasswordRequest changePasswordDto, [FromRoute] string token)
        {
            var changePasswordRequestValidation = _changePasswordValidator.Validate(changePasswordDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(changePasswordRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            var result = _resetPasswordTokenService.GetEmailFromResetPasswordToken(token);

            switch (result)
            {
                case VerifyResetPasswordToken expiredToken when expiredToken == VerifyResetPasswordToken.TokenHasExpired:
                    return Unauthorized("Token has expired");

                case VerifyResetPasswordToken invalidToken when invalidToken == VerifyResetPasswordToken.TokenValidationError:
                    return Unauthorized("Invalid token");

                default:
                    var isPasswordChanged = _userService.ChangeUserPassword((string)result, changePasswordDto.Password);

                    if (!isPasswordChanged)
                    {
                        return BadRequest("Wrong email");
                    }

                    return Ok("Password has changed successfully");
            }
        }

        [HttpPost(UserControllerEndpoints.Logout)]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies[CookieNames.RefreshToken];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                _userService.RemoveUserRefreshToken(refreshToken);
            }

            Response.Cookies.Delete(CookieNames.AccessToken);
            Response.Cookies.Delete(CookieNames.RefreshToken);
            return Ok();
        }

        [HttpGet(UserControllerEndpoints.UserData)]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        public ActionResult GetUserData([FromQuery] string email)
        {
            var user = _userService.GetUserData(email);

            if (user is null) return NotFound($"Account with email {email} does not exist");
            return Ok(user);
        }

        [HttpPut(UserControllerEndpoints.UserData)]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        public ActionResult UpdateUserData([FromBody] UpdateUserRequest updateUserDto)
        {
            var updateUserRequestValidation = _updateUserValidator.Validate(updateUserDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(updateUserRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            var updatedUser = _userService.UpdateUser(updateUserDto);

            if (updatedUser is null) return NotFound($"User with id {updateUserDto.Id} does not exist");
            return Ok(updatedUser);
        }
    }
}