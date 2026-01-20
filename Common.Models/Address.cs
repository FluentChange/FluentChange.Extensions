using System;
using System.Text;

namespace FluentChange.Extensions.Common.Models
{
    public class Address
    {
        public string Street { get; set; }
        public string Nr { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
    }

    public class ComplexAddress
    {
        public AddressType Type { get; set; }
        public string? Addition { get; set; }
        public string? StreetAndNumber { get; set; }
        public string? Postbox { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime? ValidFrom { get; set; }
    }

    public enum AddressType
    {
        Street = 0,
        POBox = 1,
        LargeCustomer = 3,
    }
}
