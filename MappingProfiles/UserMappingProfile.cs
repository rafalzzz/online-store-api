using AutoMapper;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Responses;

namespace OnlineStoreAPI.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UpdateUserDto>();
            CreateMap<UserAddress, UserAddressDto>();
        }
    }
}