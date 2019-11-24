using Dlid.MiHome.Protocol.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Protocol
{


    public class MiHomeResponse
    {
        private byte[] _data;
        private MiHomeToken _miToken;

        internal MiHomeResponse(MiHomeToken token, byte[] data)
        {
            _data = data;
            _miToken = token;
            Success = ParseResponse();
        }

        /// <summary>
        /// Parse the incoming data
        /// </summary>
        /// <returns></returns>
        private bool ParseResponse()
        {
            if (_data != null && _data.Length > 0)
            {

                using (var content = new ByteList(_data))
                {
                    // Receive the device id from the response
                    DeviceId = content.Skip(8).Take(4).ToArray();

                    var timestamp = content.ReadInt32LE(12, 4);
                    if (timestamp > 0)
                    {
                        Timestamp = new ServerTimestamp(timestamp);
                    }

                    var encrypted = content.Skip(32);
                    if (encrypted.Count() > 0)
                    {
                        var decrypted = AESHelper.Decrypt(_miToken.Key, _miToken.InitializationVector, encrypted.ToArray());
                        ResponseText = ASCIIEncoding.ASCII.GetString(decrypted);
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to parse the specified jpath as the given type
        /// </summary>
        /// <typeparam name="T">The type to convert into</typeparam>
        /// <param name="jpath">The ResponseText jpath</param>
        /// <returns>The parsed object or default(T) if not successful</returns>
        public T As<T>(string jpath = "$.result")
        {
            if (!Success || !string.IsNullOrEmpty(ResponseText))
            {
                var jObject = JObject.Parse(ResponseText);
                var jToken = jObject.SelectToken(jpath);
                if (jToken != null)
                {
                    return jToken.ToObject<T>();
                }
            }
            return default(T);
        }

        /// <summary>
        /// Returns true if result is an array of strings and the first item equals "ok"
        /// </summary>
        public bool IsOkResult()
        {
            var resultStrings = As<List<string>>();
            return resultStrings?.Count == 0 && resultStrings[0] == "ok";
        }
        
        internal MiHomeResponse()
        {
            Success = false;
        }

        public bool Success { get; private set; }

        internal ServerTimestamp Timestamp { get; private set; }

        public byte[] DeviceId { get; private set; }

        public string ResponseText { get; private set; }
    }
}
