
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
        private readonly IPasswordHasher _passwordHasher;

        public UserService(OnlineStoreDbContext dbContext, IMapper mapper, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }
        public int CreateUser(RegisterUserDto registerUserDto)
        {
            var passwordHash = _passwordHasher.Hash(registerUserDto.Password);

            var newUser = new User
            {
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = registerUserDto.Email,
                Password = passwordHash,
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return newUser.Id;
        }
    }
}