
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Models;
using AutoMapper;

namespace OnlineStoreAPI.Services
{
    public interface IUserService
    {
        int CreateUser(RegisterUserDto userDto);
    }

    public class UserService : IUserService
    {
        private readonly OnlineStoreDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(OnlineStoreDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public int CreateUser(RegisterUserDto registerUserDto)
        {
            var newUser = _mapper.Map<User>(registerUserDto);
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return newUser.Id;
        }
    }
}