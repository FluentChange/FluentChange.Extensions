using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> Copy(this Dictionary<string, string> routeParams)
        {
            var copy = new Dictionary<string, string>();
            foreach (var p in routeParams)
            {
                copy.Add(p.Key, p.Value);
            }
            return copy;
        }
    }
}
