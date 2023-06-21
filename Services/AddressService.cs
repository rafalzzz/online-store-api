using OnlineStoreAPI.Entities;
using AutoMapper;
using OnlineStoreAPI.Responses;
using OnlineStoreAPI.Requests;

namespace OnlineStoreAPI.Services
{
    public interface IAddressService
    {
        List<UserAddressDto> GetUserAddresses(int id);
        UserAddressDto AddAddress(int userId, AddAddressRequest addAddressDto);
        UserAddressDto? GetAddress(int userId, int id);
        UserAddressDto? UpdateUserAddress(int userId, int id, AddAddressRequest updateAddressDto);
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



        public List<UserAddressDto> GetUserAddresses(int id)
        {
            var addresses = _dbContext.UserAddresses
            .Where(address => address.UserId == id)
            .ToList();

            List<UserAddressDto> addressesDtos = _mapper.Map<List<UserAddressDto>>(addresses);
            return addressesDtos;
        }

        public UserAddressDto AddAddress(int userId, AddAddressRequest addAddressDto)
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

            UserAddressDto addressesDto = _mapper.Map<UserAddressDto>(newAddress);
            return addressesDto;
        }

        private UserAddress? GetUserAddress(int userId, int id)
        {
            var address = _dbContext.UserAddresses
            .FirstOrDefault(address => address.UserId == userId && address.Id == id);

            return address;
        }

        public UserAddressDto? GetAddress(int userId, int id)
        {
            var address = GetUserAddress(userId, id);

            if (address is null) return null;

            UserAddressDto addressDto = _mapper.Map<UserAddressDto>(address);
            return addressDto;
        }

        private UserAddressDto? UpdateAddress(UserAddress address, AddAddressRequest updateAddressDto)
        {
            address.AddressName = updateAddressDto.AddressName;
            address.Country = updateAddressDto.Country;
            address.City = updateAddressDto.City;
            address.Address = updateAddressDto.Address;
            address.PostalCode = updateAddressDto.PostalCode;
            address.PhoneNumber = updateAddressDto.PhoneNumber;

            _dbContext.SaveChanges();

            UserAddressDto updatedAddressDto = _mapper.Map<UserAddressDto>(address);

            return updatedAddressDto;
        }

        public UserAddressDto? UpdateUserAddress(int userId, int id, AddAddressRequest updateAddressDto)
        {
            var address = GetUserAddress(userId, id);

            if (address is null) return null;

            UserAddressDto? updatedAddressDto = UpdateAddress(address, updateAddressDto);
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