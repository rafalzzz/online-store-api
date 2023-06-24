using AutoMapper;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Responses;

namespace OnlineStoreAPI.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<UserAddress, AddressResponseDto>();
        }
    }
}