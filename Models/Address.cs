namespace HamaraCommerce.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Home";
        public string FullName { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = "Punjab";
        public string ZipCode { get; set; } = string.Empty;
        public string PostalCode { get => ZipCode; set => ZipCode = value; }
        public string Country { get; set; } = "Pakistan";
        public bool IsDefault { get; set; } = true;
    }
}
