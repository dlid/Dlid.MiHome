using Dlid.MiHome.Exceptions;
using Dlid.MiHome.Protocol.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    internal class MiHomeToken
    {
        public byte[] Key { get; private set; }
        public byte[] InitializationVector { get; private set; }
        public byte[] Token { get; private set; }

        public MiHomeToken(string token)
        {

            if (string.IsNullOrEmpty(token))
            {
                throw new MiTokenException("No token was provided");
            }

            Token = HexStringToByteArray(token);
            CalculateTokenVector();
        }

        private void CalculateTokenVector()
        {
            Key = MD5Helper.Hash(Token);

            // Join MD5 of key and the token itself to get the vector
            var ivContent = ByteList.Join(Key, Token);

            InitializationVector = MD5Helper.Hash(ivContent.ToArray());
        }

        private byte[] HexStringToByteArray(string hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            } catch (FormatException fe)
            {
                if (fe.Message.Contains("Could not find any recognizable digits"))
                {
                    throw new MiTokenException("Token must only contain HEX characters", fe);
                } else
                {
                    throw;
                }
            }
        }

    }
}
