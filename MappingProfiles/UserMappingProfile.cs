using AutoMapper;
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Models;

namespace OnlineStoreAPI.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<RegisterUserDto, User>();
        }
    }
}