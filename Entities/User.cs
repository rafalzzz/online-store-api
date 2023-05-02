using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreAPI.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public virtual List<UserAddress> UserAddresses { get; set; }
    }
}