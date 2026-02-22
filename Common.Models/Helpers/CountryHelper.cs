
namespace FluentChange.Extensions.Common.Models.Helpers
{
    public static class AddressHelper
    {
        public static bool IsEmpty(this ComplexAddress address)
        {
            return string.IsNullOrEmpty(address.StreetAndNumber)
                && string.IsNullOrEmpty(address.City)
                && string.IsNullOrEmpty(address.Zip)
                && string.IsNullOrEmpty(address.Country)
                && string.IsNullOrEmpty(address.Addition);
        }
    }
    public static class CountryHelper
    {
        public static string FixCountry(this string value)
        {

            switch (value)
            {
                case "AT": return "Österreich";
                case "BG": return "Bulgarien";
                case "CH": return "Schweiz";
                case "CZ": return "Tschechien";
                case "D": return "Deutschland";
                case "DE": return "Deutschland";
                case "Deutschland": return value;
                case "DK": return "Dänemark";
                case "ES": return "Spanien";
                case "FI": return "Finnland";
                case "FR": return "Frankreich";
                case "GB": return "Großbritannien";
                case "HU": return "Ungarn";
                case "IT": return "Italien";
                case "Italien": return value;
                case "L": return "Luxemburg";
                case "LI": return "Liechtenstein";
                case "LT": return "Litauen";
                case "LU": return "Luxemburg";
                case "Luxemburg": return value;
                case "NL": return "Niederlande";
                case "NO": return "Norwegen";
                case "Österreich": return value;
                case "PL": return "Polen";
                case "Po": return "Polen";
                case "RO": return "Rumänien";
                case "SE": return "Schweden";
                case "SI": return "Slowenien";
                case "SK": return "Slowakische Republik";



                default: return "unknown!!!"; // throw new Exception("Country not supported: " + value);
            }

        }
    }
}
