using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Dlid.MiHome.Protocol.Helpers
{
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
