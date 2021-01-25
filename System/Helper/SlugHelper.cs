using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentChange.Extensions.System.Helper
{
    public static class SlugHelper
    {
        public static string MakeSlug(string name)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9-]");
            name = rgx.Replace(name, "");
            return name.ToLower();
        }
    }
}
