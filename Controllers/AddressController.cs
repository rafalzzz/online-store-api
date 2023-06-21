using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FluentValidation;
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
        private readonly IValidator<AddAddressRequest> _addAddressValidator;

        public AddressController(
            IAddressService addressService,
            IRequestValidationService requestValidationService,
            IValidator<AddAddressRequest> addAddressValidator
        )
        {
            _addressService = addressService;
            _requestValidationService = requestValidationService;
            _addAddressValidator = addAddressValidator;
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

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult GetAddress([FromRoute] int addressId)
        {
            int userId = GetUserId(User.Claims);
            UserAddressDto? address = _addressService.GetAddress(userId, addressId);
            if (address is null) return NotFound();
            return Ok(address);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddAddress([FromBody] AddAddressRequest addAddressDto)
        {
            var addAddressRequestValidation = _addAddressValidator.Validate(addAddressDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(addAddressRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            int userId = GetUserId(User.Claims);
            UserAddressDto newAddress = _addressService.AddAddress(userId, addAddressDto);
            return Created($"/api/users/{newAddress.Id}", newAddress);
        }

        [HttpPut("{id}")]
        [Authorize]
        public ActionResult UpdateAddress([FromRoute] int addressId, [FromBody] AddAddressRequest updateAddressDto)
        {
            var updateAddressRequestValidation = _addAddressValidator.Validate(updateAddressDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(updateAddressRequestValidation);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            int userId = GetUserId(User.Claims);
            UserAddressDto updatedAddressDto = _addressService.UpdateUserAddress(userId, addressId, updateAddressDto);
            return Ok(updatedAddressDto);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteAddress([FromRoute] int addressId)
        {
            int userId = GetUserId(User.Claims);
            bool isDeleted = _addressService.DeleteUserAddress(userId, addressId);

            if (isDeleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}