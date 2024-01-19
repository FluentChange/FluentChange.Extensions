using System.Collections.Generic;

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
        public static Dictionary<string, object> Copy(this Dictionary<string, object> routeParams)
        {
            var copy = new Dictionary<string, object>();
            foreach (var p in routeParams)
            {
                copy.Add(p.Key, p.Value);
            }
            return copy;
        }
    }
}
