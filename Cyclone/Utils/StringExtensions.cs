using System.Security.Cryptography;
using System.Text;

namespace Cyclone.Utils
{
    internal static class StringExtensions
    {
        internal static string GenerateHash( this string s )
        {
            var crypto256 = new SHA256CryptoServiceProvider();
            byte[] hash256Value = crypto256.ComputeHash(Encoding.UTF8.GetBytes(s));
            
            var hashedText = new StringBuilder();
            foreach ( byte t in hash256Value )
            {
                hashedText.AppendFormat("{0:X2}", t);
            }
            return hashedText.ToString();
        }
    }
}
