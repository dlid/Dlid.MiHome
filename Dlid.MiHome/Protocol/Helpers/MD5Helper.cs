using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Dlid.MiHome.Protocol.Helpers
{
    /// <summary>
    /// Helper class to calculate MD5 hash of byte array
    /// </summary>
    internal class MD5Helper
    {
        public static byte[] Hash(byte[] input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(input);
                return result;
            }
        }
    }
}
