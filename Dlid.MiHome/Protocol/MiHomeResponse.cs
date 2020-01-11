using Dlid.MiHome.Protocol.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Protocol
{

    /// <summary>
    /// A parser of the device response and a helper to retreive the data
    /// </summary>
    public class MiHomeResponse
    {
        private byte[] _data;
        private MiHomeToken _miToken;
        private ILogger _log;

        /// <summary>
        /// Create a new instance of a MiHomeResponse 
        /// </summary>
        /// <param name="logger">The ILogger instance to use for logging</param>
        /// <param name="token">The Device token</param>
        /// <param name="data">The data received from the device</param>
        internal MiHomeResponse(ILogger logger, MiHomeToken token, byte[] data)
        { 
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _data = data;
            _miToken = token;
            Success = ParseResponse();
        }

        /// <summary>
        /// Parse the incoming data
        /// </summary>
        /// <returns>True if a response was successfully parsed</returns>
        private bool ParseResponse()
        {
            if (_data != null && _data.Length > 0)
            {
                _log.Log(LogLevel.Debug, $"Parsing response ({_data.Length} bytes)");
                using (var content = new ByteList(_data))
                {
                    // Receive the device id from the response
                    DeviceId = content.Skip(8).Take(4).ToArray();
                    TypeId = content.Skip(8).Take(2).ToArray();
                    SerialId = content.Skip(10).Take(2).ToArray();
                    Token = content.Skip(16).Take(16).ToArray() ;
                    _log.Log(LogLevel.Debug, $"DeviceID found in response");

                    var timestamp = content.ReadInt32LE(12, 4);
                    if (timestamp > 0)
                    {
                        Timestamp = new ServerTimestamp(timestamp);
                        _log.Log(LogLevel.Debug, $"Timestamp {timestamp} found in response");
                    }

                    var encrypted = content.Skip(32);
                    if (encrypted.Count() > 0)
                    {
                        using (_log.BeginScope($"Decrypting response ({encrypted.Count()} bytes)"))
                        {
                            try
                            {
                                var decrypted = AESHelper.Decrypt(_miToken.Key, _miToken.InitializationVector, encrypted.ToArray());
                                ResponseText = ASCIIEncoding.ASCII.GetString(decrypted);
                                _log.LogDebug("Response decrypted: " + ResponseText);
                            } catch (Exception ex)
                            {
                                _log.LogError(ex, "Error decrypting response");
                            }
                        }
                    }
                    else
                    {
                        _log.Log(LogLevel.Debug, "Response body was empty");
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
            if (Success && !string.IsNullOrEmpty(ResponseText))
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

        public byte[] TypeId { get; private set; }
        public byte[] DeviceId { get; private set; }
        public byte[] SerialId { get; private set; }
        public byte[] Token { get; private set; }

        public string ResponseText { get; private set; }
    }
}
