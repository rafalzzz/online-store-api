using Microsoft.AspNetCore.Mvc;
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

            return Created($"/api/users/{id}", null);
        }
    }
}