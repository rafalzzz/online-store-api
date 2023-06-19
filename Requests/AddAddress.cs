namespace OnlineStoreAPI.Requests
{
    public class AddAddressRequest
    {
        public string AddressName { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}