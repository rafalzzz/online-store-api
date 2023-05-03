using Microsoft.AspNetCore.Mvc;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Services;

namespace OnlineStoreAPI.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public ActionResult CreateUser([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = _userService.CreateUser(registerUserDto);

            if (id is null)
            {
                return Conflict(ErrorMessages.RegisterUserEmailError);
            }

            return Created($"/api/users/{id}", null);
        }

        [HttpPost("login")]
        public ActionResult LoginUser([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool? userIsVerified = _userService.VerifyUser(loginUserDto);

            return userIsVerified switch
            {
                null => NotFound(ErrorMessages.LoginUserWrongEmailError),
                false => BadRequest(ErrorMessages.LoginUserWrongPasswordError),
                true => Ok(),
            };
        }
    }
}