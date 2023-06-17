using OnlineStoreAPI.Entities;
using Microsoft.EntityFrameworkCore;
using OnlineStoreAPI.Enums;

namespace OnlineStoreAPI.Services
{
    public class UserSeeder
    {
        private readonly ModelBuilder _modelBuilder;
        private readonly IPasswordHasher _passwordHasher;
        public UserSeeder(ModelBuilder modelBuilder)
        {
            this._modelBuilder = modelBuilder;
            _passwordHasher = new PasswordHasher();
        }

        private IEnumerable<User> GetUsers()
        {
            var users = new List<User>()
            {
                new User()
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@test.com",
                    Password = _passwordHasher.Hash("Admin123!"),
                    Role = UserRole.Admin,
                    RefreshToken = "",
                },
                new User()
                {
                    Id = 2,
                    FirstName = "Janusz",
                    LastName = "Kowalski",
                    Email = "j.kowalski@test.com",
                    Password = _passwordHasher.Hash("Test123!"),
                    Role = UserRole.User,
                    RefreshToken = "",
                }

            };

            return users;
        }

        private IEnumerable<UserAddress> GetUserAddresses()
        {
            var userAddresses = new List<UserAddress>()
            {
                new UserAddress()
                {
                    Id = 3,
                    AddressName = "Admin",
                    Country = "Admin",
                    City = "Admin",
                    Address = "Admin",
                    PostalCode = "Admin",
                    PhoneNumber = "Admin",
                    UserId = 1,
                },
                new UserAddress()
                {
                    Id = 4,
                    AddressName = "main",
                    Country = "Poland",
                    City = "Warsaw",
                    Address = "Test 1/1",
                    PostalCode = "00-000",
                    PhoneNumber = "+48 123 456 789",
                    UserId = 2,
                },
            };

            return userAddresses;
        }

        public void Seed()
        {
            #region UsersSeeder
            var users = GetUsers();
            _modelBuilder.Entity<User>().HasData(users);
            #endregion

            #region UserAddressSeeder
            var userAddresses = GetUserAddresses();
            _modelBuilder.Entity<UserAddress>().HasData(userAddresses);
            #endregion
        }
    }
}