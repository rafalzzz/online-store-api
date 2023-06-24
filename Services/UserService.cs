using OnlineStoreAPI.Entities;
using OnlineStoreAPI.Requests;
using OnlineStoreAPI.Responses;
using AutoMapper;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Middleware;

namespace OnlineStoreAPI.Services
{
    public interface IUserService
    {
        User GetUserById(int id);
        string GetUserRoleDescription(UserRole value);
        bool CheckIfEmailExist(string email);
        int? CreateUser(RegisterRequest userDto);
        (VerifyUserError error, User user, bool isError) VerifyUser(LoginRequest loginUserDto);
        bool SaveUserRefreshToken(string token, User user);
        bool CheckUserRefreshToken(User? user, string token);
        void RemoveUserRefreshToken(string token);
        bool ChangeUserPassword(string email, string password);
        UserResponseDto? GetUserData(string email);
        UserResponseDto? UpdateUser(UserRequestDto UserResponseDto);
    }

    public class UserService : IUserService
    {
        private readonly OnlineStoreDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IResetPasswordTokenService _resetPasswordTokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public UserService(
            OnlineStoreDbContext dbContext,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IRefreshTokenService refreshTokenService,
            IResetPasswordTokenService resetPasswordTokenService,
            IEmailService emailService,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _refreshTokenService = refreshTokenService;
            _resetPasswordTokenService = resetPasswordTokenService;
            _emailService = emailService;
            _logger = logger;
        }

        public User GetUserByEmail(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Email == email);
            return user;
        }

        public User GetUserById(int id)
        {
            var user = _dbContext.Users.FirstOrDefault(user => user.Id == id);
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
                RefreshToken = "",
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return newUser.Id;
        }

        public string GetUserRoleDescription(UserRole value)
        {
            if (!Enum.IsDefined(typeof(UserRole), value))
            {
                throw new ArgumentException("Provided value does not correspond to a UserRole");
            }

            UserRole role = (UserRole)value;
            return role.GetDescription();
        }

        public (VerifyUserError error, User user, bool isError) VerifyUser(LoginRequest loginUserDto)
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

                return (VerifyUserError.NoError, user, false);
            }
            catch (ArgumentException exception)
            {
                _logger.LogError(exception.Message);
                return (VerifyUserError.WrongRole, null, true);
            }
        }

        public bool SaveUserRefreshToken(string? token, User user)
        {
            user.RefreshToken = token;
            _dbContext.SaveChanges();

            return true;
        }

        public bool CheckUserRefreshToken(User? user, string token)
        {
            if (user is null)
            {
                return false;
            }

            return token == user.RefreshToken;
        }

        public void RemoveUserRefreshToken(string token)
        {
            var userId = _refreshTokenService.GetUserIdFromRefreshToken(token);
            if (userId is null) return;

            var user = GetUserById((int)userId);
            if (user is null) return;

            user.RefreshToken = "";
            _dbContext.SaveChanges();
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

        public UserResponseDto? GetUserData(string email)
        {
            var user = GetUserByEmail(email);

            if (user is null) return null;

            UserResponseDto userDto = _mapper.Map<UserResponseDto>(user);
            return userDto;
        }

        public UserResponseDto? UpdateUser(UserRequestDto UserResponseDto)
        {
            var user = GetUserById(UserResponseDto.Id);

            if (user is null) return null;

            user.FirstName = UserResponseDto.FirstName;
            user.LastName = UserResponseDto.LastName;
            user.Email = UserResponseDto.Email;
            user.Role = UserResponseDto.Role;

            _dbContext.SaveChanges();

            UserResponseDto userDto = _mapper.Map<UserResponseDto>(user);

            return userDto;
        }
    }
}