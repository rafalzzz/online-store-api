using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OnlineStoreAPI.Responses;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Controllers
{
    [Route(ControllerRoutes.AddressController)]
    public class AddressController : ControllerBase
    {
        IAddressService _addressService;
        private readonly IRequestValidationService _requestValidationService;

        public AddressController(
            IAddressService addressService,
            IRequestValidationService requestValidationService
        )
        {
            _addressService = addressService;
            _requestValidationService = requestValidationService;
        }

        private int GetUserId(IEnumerable<Claim> claims)
        {
            string userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetUserAddresses()
        {
            int userId = GetUserId(User.Claims);
            List<UserAddressDto> addresses = _addressService.GetUserAddresses(userId);
            return Ok(addresses);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddAddress([FromBody] AddAddressRequest addAddressDto)
        {
            int userId = GetUserId(User.Claims);
            UserAddressDto newAddress = _addressService.AddAddress(userId, addAddressDto);
            return Created($"/api/users/{newAddress.Id}", newAddress);
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult GetAddress([FromRoute] int id)
        {
            int userId = GetUserId(User.Claims);
            UserAddressDto? address = _addressService.GetAddress(userId, id);
            if (address is null) return NotFound();
            return Ok(address);
        }
    }
}