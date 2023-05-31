using System.ComponentModel;

namespace OnlineStoreAPI.Enums
{
    public enum UserRole
    {
        [Description("admin")]
        Admin,
        [Description("user")]
        User
    }
}