using OnlineStoreAPI.Entities;
using AutoMapper;
using OnlineStoreAPI.Responses;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Services
{
    public interface IAddressService
    {
        List<AddressResponseDto> GetUserAddresses(int id);
        AddressResponseDto AddAddress(int userId, AddressRequestDto addAddressDto);
        AddressResponseDto? GetAddress(int userId, int id);
        AddressResponseDto? UpdateUserAddress(int userId, int id, AddressRequestDto updateAddressDto);
        bool DeleteUserAddress(int userId, int id);
    }

    public class AddressService : IAddressService
    {
        private readonly OnlineStoreDbContext _dbContext;
        private readonly IMapper _mapper;

        public AddressService(
            OnlineStoreDbContext dbContext,
            IMapper mapper
            )
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }



        public List<AddressResponseDto> GetUserAddresses(int id)
        {
            var addresses = _dbContext.UserAddresses
            .Where(address => address.UserId == id)
            .ToList();

            List<AddressResponseDto> addressesDtos = _mapper.Map<List<AddressResponseDto>>(addresses);
            return addressesDtos;
        }

        public AddressResponseDto AddAddress(int userId, AddressRequestDto addAddressDto)
        {

            var newAddress = new UserAddress
            {
                AddressName = addAddressDto.AddressName,
                Country = addAddressDto.Country,
                City = addAddressDto.City,
                Address = addAddressDto.Address,
                PostalCode = addAddressDto.PostalCode,
                PhoneNumber = addAddressDto.PhoneNumber,
                UserId = userId,
            };

            _dbContext.UserAddresses.Add(newAddress);
            _dbContext.SaveChanges();

            AddressResponseDto addressesDto = _mapper.Map<AddressResponseDto>(newAddress);
            return addressesDto;
        }

        private UserAddress? GetUserAddress(int userId, int id)
        {
            var address = _dbContext.UserAddresses
            .FirstOrDefault(address => address.UserId == userId && address.Id == id);

            return address;
        }

        public AddressResponseDto? GetAddress(int userId, int id)
        {
            var address = GetUserAddress(userId, id);

            if (address is null) return null;

            AddressResponseDto addressDto = _mapper.Map<AddressResponseDto>(address);
            return addressDto;
        }

        private AddressResponseDto? UpdateAddress(UserAddress address, AddressRequestDto updateAddressDto)
        {
            address.AddressName = updateAddressDto.AddressName;
            address.Country = updateAddressDto.Country;
            address.City = updateAddressDto.City;
            address.Address = updateAddressDto.Address;
            address.PostalCode = updateAddressDto.PostalCode;
            address.PhoneNumber = updateAddressDto.PhoneNumber;

            _dbContext.SaveChanges();

            AddressResponseDto updatedAddressDto = _mapper.Map<AddressResponseDto>(address);

            return updatedAddressDto;
        }

        public AddressResponseDto? UpdateUserAddress(int userId, int id, AddressRequestDto updateAddressDto)
        {
            var address = GetUserAddress(userId, id);

            if (address is null) return null;

            AddressResponseDto? updatedAddressDto = UpdateAddress(address, updateAddressDto);
            return updatedAddressDto;
        }

        public bool DeleteUserAddress(int userId, int id)
        {
            var address = GetUserAddress(userId, id);

            if (address is null) return false;

            _dbContext.UserAddresses.Remove(address);
            _dbContext.SaveChanges();

            return true;
        }
    }
}