using Dlid.MiHome.Protocol.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    public class MiHomeRequest
    {

        private object _payload;
        private MiHomeToken _token;
        private byte[] _deviceId;
        private ServerTimestamp _serverStamp;

        public NetworkOptions NetworkOptions { get; private set; }

        /// <summary>
        /// Creates a Handshake request
        /// </summary>
        public MiHomeRequest(NetworkOptions options)
        {
            NetworkOptions = options;
        }

        internal MiHomeRequest(MiHomeToken token, byte[] deviceId, ServerTimestamp ts, NetworkOptions options, object objectBody)
        {
            _payload = objectBody;
            _token = token;
            _deviceId = deviceId;
            _serverStamp = ts;
            NetworkOptions = options;
        }

        internal MiHomeRequest(MiHomeToken token, byte[] deviceId, ServerTimestamp ts, NetworkOptions options, string jsonBody)
        {
            _payload = JsonConvert.DeserializeObject(jsonBody);
            _token = token;
            _deviceId = deviceId;
            _serverStamp = ts;
            NetworkOptions = options;
        }

        internal void UpdateDeviceId(byte[] deviceId)
        {
            _deviceId = deviceId;
        }
        internal void UpdateTimestamp(ServerTimestamp stamp)
        {
            _serverStamp = stamp;
        }


        internal int RequestId { get; set; }

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
                o.Add("id", RequestId);
            }
            return o.ToString(Formatting.None);
        }

        public bool IsHandshake => _payload == null;

        public byte[] GetBytes()
        {
            return IsHandshake ? GetHandshakeBytes() : GetPayloadBytes();
        }

        internal byte[] GetHandshakeBytes()
        {
            using(var content = new ByteList())
            {
                // Magic number and packet size (0x20 = 32 bytes since there is no payload)
                content.Add(0x21, 0x31, 0, 0x20);

                // Fill up the rest of the 32 bytes with 0xff
                content.Repeat(0xff, 28, 4);

                return content.ToBinaryASCIIArray();
            }
        }

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
            var encryptedPayload = AESHelper.Encrypt(_token.Key, _token.InitializationVector, jsonBody);

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
