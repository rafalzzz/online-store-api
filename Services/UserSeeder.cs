using OnlineStoreAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreAPI.Services
{
    public class UserSeeder
    {
        private readonly ModelBuilder _modelBuilder;
        public UserSeeder(ModelBuilder modelBuilder)
        {
            this._modelBuilder = modelBuilder;
        }

        private IEnumerable<User> GetUsers()
        {
            var users = new List<User>()
            {
                new User()
                {
                    Id = 1,
                    FirstName = "Janusz",
                    LastName = "Kowalski",
                    Email = "j.kowalski@test.com",
                    Password = "Test123!",
                },
            };

            return users;
        }

        private IEnumerable<UserAddress> GetUserAddresses()
        {
            var userAddresses = new List<UserAddress>()
            {
                new UserAddress()
                {
                    Id = 2,
                    AddressName = "main",
                    Country = "Poland",
                    City = "Warsaw",
                    Address = "Test 1/1",
                    PostalCode = "00-000",
                    PhoneNumber = "+48 123 456 789",
                    UserId = 1,
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