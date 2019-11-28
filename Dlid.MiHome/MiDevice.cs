using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Dlid.MiHome.Protocol;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Dlid.MiHome
{
    public class MiDevice : IDisposable
    {
        internal byte[] _deviceId;
        private string _ipAddress;
        private ILogger _log;
        internal MiHomeToken _miToken;
        internal int _requestId = 0;
        internal NetworkConnection _socket;

        public NetworkOptions NetworkOptions { get; set; } = new NetworkOptions();

        /// <summary>
        /// The server timestamp is received from the device responses
        /// In the requests we will use this value and add the number of seconds that has passed since we received the last response
        /// </summary>
        internal ServerTimestamp _serverTimestamp;

        public MiDevice(string IPAddress, string Token, ILogger logger = null)
        {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            _ipAddress = IPAddress;
            _miToken = new MiHomeToken(Token, _log);
            
            _log.Log(LogLevel.Trace, $"Created Device {IPAddress} ");

            // Unique enough Id base
            _requestId = Guid.NewGuid().GetHashCode() / 2;
        }

        /// <summary>
        /// Make sure settings are set, a connection exists and a handshake has been made
        /// </summary>
        private void EnsureConnection()
        {
            if (NetworkOptions == null)
            {
                NetworkOptions = new NetworkOptions();
            }

            if (_socket == null)
            {
                _log.Log(LogLevel.Trace, $"Connecting to {this._ipAddress}:{NetworkOptions.NetworkPort}");
                _socket = new NetworkConnection(_log, NetworkOptions);

                _socket.Connect(_ipAddress, NetworkOptions.NetworkPort);
                _log.Log(LogLevel.Trace, $"Connected");
            }

            if (InNeedOfHandshake)
            {
                var handshakeResponse = Handshake();
                if (!handshakeResponse.Success)
                {
                    var err = new Exception("Handshake failed");
                    _log.LogError(err, "Handshake failed");
                    throw err;
                }
            }
        }

        /// <summary>
        /// Send a Handshake request
        /// </summary>
        /// <returns>The response from the handshake</returns>
        private MiHomeResponse Handshake()
        {
            var miRequest = new MiHomeRequest(NetworkOptions);
            return Send(miRequest);
        }

        /// <summary>
        /// Check if a handshake is required
        /// </summary>
        private bool InNeedOfHandshake
        {
            get
            {
                var secondsSinceLastHandhake = _serverTimestamp != null ? (DateTime.Now - _serverTimestamp.ReceivedTime).TotalSeconds : NetworkOptions.HandshakeEvery.TotalSeconds + 1;
                return secondsSinceLastHandhake > NetworkOptions.HandshakeEvery.TotalSeconds;
            }
        }

        /// <summary>
        /// Create an id for the next request
        /// </summary>
        private void NextId()
        {
            if (_requestId + 100 > int.MaxValue) {
                _requestId = Guid.NewGuid().GetHashCode() / 2;
            } 
            _requestId += 100;
        }

        /// <summary>
        /// Send the content of MiHomeRequest and wait for a response
        /// </summary>
        /// <param name="request">The Request to send</param>
        /// <returns>The parsed and decrypted response from the device</returns>
        private MiHomeResponse Send(MiHomeRequest request)
        {
            if (!request.IsHandshake) { 
                EnsureConnection();
                NextId();

                // Update the request with the updated values from the handshake
                request.UpdateDeviceId(_deviceId);
                request.UpdateTimestamp(_serverTimestamp);
                request.RequestId = _requestId;
            }

            int retryCount = request.NetworkOptions.RetryCount;
            do
            {
                var requestPayload = request.GetBytes();
                var responsePayload = _socket.Send(requestPayload, request.NetworkOptions);
                if (responsePayload != null)
                {
                    var miResponse = new MiHomeResponse(_log, _miToken, responsePayload);
                    if (miResponse.Success)
                    {
                        // We need Device ID and server timestamp for subsequent requests
                        _serverTimestamp = miResponse.Timestamp;
                        if (_deviceId == null && miResponse.DeviceId != null)
                        {
                            _deviceId = miResponse.DeviceId;
                        }
                        return miResponse;
                    }
                }
                request.RequestId++;
                retryCount--;
                if (NetworkOptions.RetryDelay.TotalMilliseconds > 0)
                {
                    Thread.Sleep(NetworkOptions.HandshakeEvery);
                }
            } while (retryCount > 0);

            return new MiHomeResponse();
        }

        /// <summary>
        /// Send a request with the given method name and parameters
        /// </summary>
        /// <param name="methodName">The method name</param>
        /// <param name="parameters">Any parameters that the request requires</param>
        /// <returns>The parsed and decrypted response from the device</returns>
        public MiHomeResponse Send(string methodName, params object[] parameters)
        {
            var miRequest = new MiHomeRequest(_miToken, _deviceId, _serverTimestamp, NetworkOptions, new
            {
                method = methodName,
                @params = parameters
            }, _log);
            return Send(miRequest);
        }

        /// <summary>
        /// Send a custom request with the given data
        /// </summary>
        /// <param name="data">The object that will be serialized into the JSON body for the request. If you do not send an id property, the library will handle that for you (recommended)</param>
        /// <returns>The parsed and decrypted response from the device</returns>
        public MiHomeResponse Send(object data)
        {
            var myRequest = new MiHomeRequest(_miToken, _deviceId, _serverTimestamp, NetworkOptions, data, _log);
            return Send(myRequest);
        }


        /// <summary>
        /// Close any open connection and dispose the object
        /// </summary>
        public void Dispose()
        {
            if (this._socket != null)
            {
                this._socket.Close();
            }
        }
    }
}
