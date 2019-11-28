using Dlid.MiHome.Protocol.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    /// <summary>
    /// The internal request that will build the packet to send to the device
    /// </summary>
    internal class MiHomeRequest
    {
        private byte[] _deviceId;
        private ILogger _log;
        private object _payload;
        private ServerTimestamp _serverStamp;
        private MiHomeToken _token;

        /// <summary>
        /// The network options to use for this request
        /// </summary>
        public NetworkOptions NetworkOptions { get; private set; }

        /// <summary>
        /// Creates a Handshake request - without a body payload
        /// </summary>
        public MiHomeRequest(NetworkOptions options, ILogger logger = null)
        {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            NetworkOptions = options;
        }
        
        /// <summary>
        /// Create a new request
        /// </summary>
        /// <param name="token">The device token to use for encrypting the payload</param>
        /// <param name="deviceId">The device id that was received from a previous handshake</param>
        /// <param name="ts">The lastest timestamp received from the device</param>
        /// <param name="options">Network options to use for this request</param>
        /// <param name="objectBody">The body that will be serialized to JSON and sent to the device</param>
        internal MiHomeRequest(MiHomeToken token, byte[] deviceId, ServerTimestamp ts, NetworkOptions options, object objectBody, ILogger logger = null)
        {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _payload = objectBody;
            _token = token;
            _deviceId = deviceId;
            _serverStamp = ts;
            NetworkOptions = options;
        }

        /// <summary>
        /// Set a new DeviceID that was received from a previous response
        /// </summary>
        /// <param name="deviceId"></param>
        internal void UpdateDeviceId(byte[] deviceId)
        {
            _deviceId = deviceId;
            _log.Log(LogLevel.Debug, "Request was updated with new device id");
        }

        /// <summary>
        /// Set a new Timestamp that was received from a previous response
        /// </summary>
        /// <param name="stamp"></param>
        internal void UpdateTimestamp(ServerTimestamp stamp)
        {
            _serverStamp = stamp;
            _log.Log(LogLevel.Debug, "Request was updated with new timestamp");
        }


        /// <summary>
        /// The ID of this request that will be part of the payload
        /// </summary>
        internal int RequestId { get; set; }

        /// <summary>
        /// Create the JSON string for the request
        /// </summary>
        /// <returns></returns>
        internal string CreateJson()
        {
            if (_payload == null)
            {
                return string.Empty;
            }

            var o = JObject.FromObject(_payload);
            var jId = o.SelectToken("id");
            if (jId == null)        
            {
                _log.Log(LogLevel.Debug, "Adding ID property to request: " + RequestId);
                o.Add("id", RequestId);
            }
            return o.ToString(Formatting.None);
        }

        /// <summary>
        /// Returns true if the body payload is empty
        /// </summary>
        public bool IsHandshake => _payload == null;

        /// <summary>
        /// Get the raw data for this request to send to the device
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return IsHandshake ? GetHandshakeBytes() : GetPayloadBytes();
        }

        /// <summary>
        /// Create the handshake request
        /// </summary>
        /// <returns>The binary data to send to the device</returns>
        internal byte[] GetHandshakeBytes()
        {
            using(_log.BeginScope("Creating Handshake Packet"))
            using(var content = new ByteList())
            {
                // Magic number and packet size (0x20 = 32 bytes since there is no payload)
                content.Add(0x21, 0x31, 0, 0x20);

                // Fill up the rest of the 32 bytes with 0xff
                content.Repeat(0xff, 28, 4);

                return content.ToBinaryASCIIArray();
            }
        }

        /// <summary>
        /// Create a request containing an encrypted body payload
        /// </summary>
        /// <returns>The binary data to send to the device</returns>
        internal byte[] GetPayloadBytes()
        {

            if (_token == null)
            {
                throw new Exception("Token is missing");
            }

            if (_deviceId == null)
            {
                throw new Exception("DeviceID is missing");
            }

            // Encrypt the payload using the token
            var jsonBody = CreateJson();

            byte[] encryptedPayload;

            try
            {
                encryptedPayload = AESHelper.Encrypt(_token.Key, _token.InitializationVector, jsonBody);
            } catch (Exception ex)
            {
                _log.LogError(ex, "Could not encrypt payload");
                throw;
            }

            using (var content = new ByteList())
            {
                // Create the header
                content.Add(0x21, 0x31);                                // Magic number
                content.WriteUInt16BE(32 + encryptedPayload.Length);    // Content+header length
                content.Repeat(0x00, 4);                                // Unknown1
                content.AddRange(_deviceId);                            // Device ID (received from handshake)

                // Add seconds passed to server timestamp
                var secondsPassed = (DateTime.Now - _serverStamp.ReceivedTime).TotalSeconds;

                // Write Stamp to payload
                content.WriteUInt32BE((UInt32)Math.Floor(_serverStamp.Timestamp + secondsPassed));

                // Header is done and we have the payload and the token. We can now write the MD5 checksum
                // Get MD5 checksum of [header, token payload]
                content.AddRange(ByteList.Join(content.Take(16).ToArray(), _token.Token, encryptedPayload).ToMd5());

                // Then add the encrypted payload itself
                content.AddRange(encryptedPayload);

                return content.ToBinaryASCIIArray();
            }
        }

    }
}
