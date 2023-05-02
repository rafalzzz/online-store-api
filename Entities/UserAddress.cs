namespace OnlineStoreAPI.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }
        public string AddressName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}