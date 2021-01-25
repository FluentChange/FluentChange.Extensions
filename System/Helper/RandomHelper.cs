using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class RandomHelper
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
