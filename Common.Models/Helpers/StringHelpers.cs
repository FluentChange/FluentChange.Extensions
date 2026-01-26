

using System;
using System.Linq;

namespace FluentChange.Extensions.Common.Models.Helpers
{
    public static class StringHelpers
    {
        private static string[] titles = ["Prof.", "Dr.", "Prof. Dr."];
        public static string GetTitle(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            name = name.Trim();
            foreach (var title in titles)
            {
                if (name.EndsWith(title)) return title;
            }
            return "";
        }
        public static string StripTitle(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            name = name.Trim();
            foreach (var title in titles)
            {
                if (name.EndsWith(title)) return name.Substring(0, name.Length - title.Length).Trim();
            }
            return name;
        }

        private static string[] legalTypes = ["GmbH", "GMBH", "AG", "GmbH & Co. KG", "UG", "mbH", "mbB", "UG (haftungsbeschränkt)", "PartGmbB", "GmbH &Co. KG"];
        public static string GetLegalType(string name)
        {
            name = name.Trim();
            foreach (var type in legalTypes)
            {
                if (name.EndsWith(type)) return type;
            }
            return "";
        }
        public static string StripLegalType(string name)
        {
            name = name.Trim();
            foreach (var type in legalTypes)
            {
                if (name.EndsWith(type)) return name.Substring(0, name.Length - type.Length).Trim();
            }
            return name;
        }
        public static string GetShortName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.Trim().Split(" ").First();
        }
        public static Gender GetGender(string salutation)
        {
            salutation = salutation.Trim();
            if (salutation.Contains("Herr")) return Gender.Male;            
            if (salutation.Contains("Frau")) return Gender.Female;
            if (salutation == "") return Gender.Unknown;
            throw new Exception("unknown salutation " + salutation);
        }

    }
}
