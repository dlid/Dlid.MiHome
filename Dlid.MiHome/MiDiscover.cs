using Dlid.MiHome.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Dlid.MiHome {
    public class MiDiscover : IDisposable {

        public List<MiDevice> Devices { get; private set; } = new List<MiDevice>();
        private ILogger _log;
        private string _ipAddress;
        internal UdpClient _socket;
        internal int _requestId = 0;

        public NetworkOptions NetworkOptions { get; set; } = new NetworkOptions();

        /// <summary>
        /// The server timestamp is received from the device responses
        /// In the requests we will use this value and add the number of seconds that has passed since we received the last response
        /// </summary>
        internal ServerTimestamp _serverTimestamp;

        public MiDiscover(bool netbroadcast = false, ILogger logger = null) {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            _log.Log(LogLevel.Trace, $"Discover new Devices");
            if (netbroadcast) {

            } else
                _ipAddress = "255.255.255.255";
            // Unique enough Id base
            _requestId = Guid.NewGuid().GetHashCode() / 2;
        }

        /// <summary>
        /// Make sure settings are set, a connection exists and a handshake has been made
        /// </summary>
        public void Discover() {
            if (NetworkOptions == null) {
                NetworkOptions = new NetworkOptions();
            }

            if (_socket == null) {
                _log.Log(LogLevel.Trace, $"Connecting to {this._ipAddress}:{NetworkOptions.NetworkPort}");
                _socket = new UdpClient();
            }

            var miRequest = new MiHomeRequest(NetworkOptions, _log);
            Send(miRequest);
        }

        /// <summary>
        /// Create an id for the next request
        /// </summary>
        private void NextId() {
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
        private void Send(MiHomeRequest request) {
            int retryCount = request.NetworkOptions.RetryCount;
            do {
                var requestPayload = request.GetBytes();
                System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(_ipAddress);
                System.Net.IPEndPoint remoteEP = new IPEndPoint(ipAdd, NetworkOptions.NetworkPort);
                _socket.Send(requestPayload, requestPayload.Length, remoteEP);

                _socket.Client.ReceiveTimeout = this.NetworkOptions.ReceiveTimeout.Milliseconds;
                _socket.BeginReceive(new AsyncCallback(recv), null);
                request.RequestId++;
                retryCount--;
                if (NetworkOptions.RetryDelay.TotalMilliseconds > 0) {
                    System.Threading.Thread.Sleep(NetworkOptions.ReceiveTimeout.Milliseconds + NetworkOptions.RetryDelay.Milliseconds);
                } else
                    System.Threading.Thread.Sleep(NetworkOptions.ReceiveTimeout.Milliseconds);
            } while (retryCount > 0);
        }

        private void recv(IAsyncResult res) {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 54321);
            byte[] responsePayload = _socket.EndReceive(res, ref RemoteIpEndPoint);

            if (responsePayload != null) {
                var miResponse = new MiHomeResponse(_log, new MiHomeToken("ffffffffffffffffffffffffffffffff", _log), responsePayload);
                if (miResponse.Success) {
                    // We need Device ID and server timestamp for subsequent requests
                    var newdev = new MiDevice(miResponse);
                    newdev.IpAddress = RemoteIpEndPoint.Address.ToString();
                    bool found = false;
                    foreach (var item in Devices) {
                        if (item.DeviceID == newdev.DeviceID)
                            found = true;
                    }
                    if (!found) {
                        Devices.Add(newdev);
                    }
                }
            }
            _socket.BeginReceive(new AsyncCallback(recv), null);
        }


        /// <summary>
        /// Close any open connection and dispose the object
        /// </summary>
        public void Dispose() {
            if (this._socket != null) {
                this._socket.Close();
            }
        }
    }
}
