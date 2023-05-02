using Microsoft.EntityFrameworkCore;
using OnlineStoreAPI.Helpers;
using DotNetEnv;

namespace OnlineStoreAPI.Entities
{
    public class OnlineStoreDbContext : DbContext
    {
        public OnlineStoreDbContext(DbContextOptions<OnlineStoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<UserAddress>();

            new UserSeeder(modelBuilder).Seed();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            Env.Load();

            var connectionString = Environment.GetEnvironmentVariable("ONLINE_STORE_CONNECTION_STRING");
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}