
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Models;
using AutoMapper;

namespace OnlineStoreAPI.Services
{
    public interface IUserService
    {
        int? CreateUser(RegisterUserDto userDto);
        bool? VerifyUser(LoginUserDto loginUserDto);
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

        public User GetUserByEmail(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }

        private bool CheckIfEmailExist(string email)
        {
            var user = GetUserByEmail(email);
            if (user is null) return false;
            return true;
        }

        public int? CreateUser(RegisterUserDto registerUserDto)
        {
            if (CheckIfEmailExist(registerUserDto.Email)) return null;

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

        public bool? VerifyUser(LoginUserDto loginUserDto)
        {
            var user = GetUserByEmail(loginUserDto.Email);

            if (user is null)
            {
                return null;
            }

            var isPasswordCorrect = _passwordHasher.Verify(
                    user.Password,
                    loginUserDto.Password
                );

            return isPasswordCorrect;
        }
    }
}