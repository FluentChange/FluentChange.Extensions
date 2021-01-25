using System;
using System.Security.Cryptography;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class HashHelpers
    {
        public static string CalculateMD5(string inputString)
        {
            using (var md5 = MD5.Create())
            {

                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(inputString));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
