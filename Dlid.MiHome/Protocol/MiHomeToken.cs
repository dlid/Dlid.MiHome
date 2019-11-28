using Dlid.MiHome.Exceptions;
using Dlid.MiHome.Protocol.Helpers;
using Microsoft.Extensions.Logging;
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

        ILogger _log;

        public MiHomeToken(string token, ILogger logger = null)
        {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            if (string.IsNullOrEmpty(token))
            {
                _log.LogError("No token was provided");
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
            _log.Log(LogLevel.Debug, "Key and IV was resolved for the token");
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
                    _log.LogError(fe, "Token was not a valid hex character string");
                    throw new MiTokenException("Token must only contain HEX characters", fe);
                } else
                {
                    _log.LogError(fe, "An error occured when parsing the token as a hex string");
                    throw;
                }
            }
        }

    }
}
