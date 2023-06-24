using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FluentValidation;
using OnlineStoreAPI.Responses;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;
using OnlineStoreAPI.Models;

namespace OnlineStoreAPI.Controllers
{
    [Route(ControllerRoutes.AddressController)]
    public class AddressController : ControllerBase
    {
        IAddressService _addressService;
        private readonly IRequestValidationService _requestValidationService;
        private readonly IValidator<AddressRequestDto> _addressRequestDtoValidator;

        public AddressController(
            IAddressService addressService,
            IRequestValidationService requestValidationService,
            IValidator<AddressRequestDto> addressRequestDtoValidator
        )
        {
            _addressService = addressService;
            _requestValidationService = requestValidationService;
            _addressRequestDtoValidator = addressRequestDtoValidator;
        }

        private int GetUserId(IEnumerable<Claim> claims)
        {
            string userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }

        private IEnumerable<ValidationError>? ValidateAddressRequestDto(AddressRequestDto addressDto)
        {
            var addAddressRequestValidation = _addressRequestDtoValidator.Validate(addressDto);
            var validationResultErrors = _requestValidationService.GetValidationErrorsResult(addAddressRequestValidation);

            return validationResultErrors;
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetUserAddresses()
        {
            int userId = GetUserId(User.Claims);
            List<AddressResponseDto> addresses = _addressService.GetUserAddresses(userId);
            return Ok(addresses);
        }

        [HttpGet("{addressId}")]
        [Authorize]
        public ActionResult GetUserAddress([FromRoute] int addressId)
        {
            int userId = GetUserId(User.Claims);
            AddressResponseDto? address = _addressService.GetAddress(userId, addressId);
            if (address is null) return NotFound();
            return Ok(address);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddAddress([FromBody] AddressRequestDto addressDto)
        {
            IEnumerable<ValidationError>? validationResultErrors = ValidateAddressRequestDto(addressDto);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            int userId = GetUserId(User.Claims);
            AddressResponseDto newAddress = _addressService.AddAddress(userId, addressDto);
            return Created($"/api/users/{newAddress.Id}", newAddress);
        }

        [HttpPut("{addressId}")]
        [Authorize]
        public ActionResult UpdateAddress([FromRoute] int addressId, [FromBody] AddressRequestDto addressDto)
        {
            IEnumerable<ValidationError>? validationResultErrors = ValidateAddressRequestDto(addressDto);

            if (validationResultErrors != null)
            {
                return BadRequest(validationResultErrors);
            }

            int userId = GetUserId(User.Claims);
            AddressResponseDto updatedAddressDto = _addressService.UpdateUserAddress(userId, addressId, addressDto);

            if (updatedAddressDto is null)
            {
                return NotFound();
            }

            return Ok(updatedAddressDto);
        }

        [HttpDelete("{addressId}")]
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