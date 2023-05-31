
using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Requests;
using AutoMapper;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Variables;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Middleware;

namespace OnlineStoreAPI.Services
{
    public interface IUserService
    {
        User GetUserByEmail(string email);
        bool CheckIfEmailExist(string email);
        int? CreateUser(RegisterRequest userDto);
        (VerifyUserError error, VerifiedUser userData, bool isError) VerifyUser(LoginRequest loginUserDto);
        Task SendResetPasswordToken(string email);
        bool ChangeUserPassword(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly OnlineStoreDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public UserService(
            OnlineStoreDbContext dbContext,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        public User GetUserByEmail(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }

        public bool CheckIfEmailExist(string email)
        {
            var user = GetUserByEmail(email);
            if (user is null) return false;
            return true;
        }

        public int? CreateUser(RegisterRequest registerUserDto)
        {
            if (CheckIfEmailExist(registerUserDto.Email)) return null;

            var passwordHash = _passwordHasher.Hash(registerUserDto.Password);

            var newUser = new User
            {
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = registerUserDto.Email,
                Password = passwordHash,
                Role = UserRole.User,
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return newUser.Id;
        }

        private string GetUserRoleDescription(int value)
        {
            if (!Enum.IsDefined(typeof(UserRole), value))
            {
                throw new ArgumentException("Provided value does not correspond to a UserRole");
            }

            UserRole role = (UserRole)value;
            return role.GetDescription();
        }

        public (VerifyUserError error, VerifiedUser userData, bool isError) VerifyUser(LoginRequest loginUserDto)
        {
            var user = GetUserByEmail(loginUserDto.Email);

            if (user is null)
            {
                return (VerifyUserError.EmailNoExist, null, true);
            }

            var isPasswordCorrect = _passwordHasher.Verify(
                    user.Password,
                    loginUserDto.Password
                );

            if (!isPasswordCorrect)
            {
                return (VerifyUserError.WrongPassword, null, true);
            }

            try
            {
                string userRole = GetUserRoleDescription((int)user.Role);

                VerifiedUser userData = new VerifiedUser()
                {
                    Email = user.Email,
                    Role = userRole
                };

                return (VerifyUserError.NoError, userData, false);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError(exception.Message);
                return (VerifyUserError.WrongRole, null, true);
            }
        }

        public async Task SendResetPasswordToken(string email)
        {
            string token = _jwtService.GenerateResetPasswordToken(email);

            string emailTitle = "Confirm your email";

            string clientUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.ClientUrl);
            string tokenLink = $"{clientUrl}/{token}";
            string emailMessage = $"Click on the link to confirm your email: {tokenLink}";

            await _emailService.SendEmailAsync(email, emailTitle, emailMessage);
        }

        public bool ChangeUserPassword(string email, string password)
        {
            var user = GetUserByEmail(email);

            if (user is null) return false;

            var passwordHash = _passwordHasher.Hash(password);
            user.Password = passwordHash;
            _dbContext.SaveChanges();

            return true;
        }
    }
}