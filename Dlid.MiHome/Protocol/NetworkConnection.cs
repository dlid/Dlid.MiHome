using Dlid.MiHome.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Dlid.MiHome
{
    /// <summary>
    /// The wrapper for the network socket toward the device
    /// </summary>
    internal class NetworkConnection
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private ILogger _log;
        private NetworkOptions _networkOptions;

        public NetworkConnection(ILogger logger, NetworkOptions options)
        {
            _log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _networkOptions = options;
        }

        public NetworkOptions Options { get; private set; } = new NetworkOptions();

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }


        public void Close()
        {
            if (_socket != null)
            {
                try
                {
                    _log.Log(LogLevel.Debug, "Closing socket");
                    _socket.Close();
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error closing socket");
                    throw;
                }
            }
        }

        public void Connect(string address, int port)
        {
            _socket.Connect(IPAddress.Parse(address), _networkOptions.NetworkPort);

        }

        public byte[] Send(byte[] data, NetworkOptions options)
        {
            SocketError err;

            _socket.SendTimeout = (int)options.SendTimeout.TotalMilliseconds;

            
            using (_log.BeginScope($"Sending Socket Request of {data.Length} bytes")) { 
                _socket.Send(data, 0, data.Length, SocketFlags.None, out err);
                _log.Log(LogLevel.Debug, "Send Completed with status: " + err);
            }

            _socket.ReceiveTimeout = (int)options.ReceiveTimeout.TotalMilliseconds;

            try
            {
                using (_log.BeginScope($"Waiting for Response"))
                {
                    int readBytes = _socket.Receive(state.buffer, 0, bufSize, SocketFlags.None, out err);
                    _log.Log(LogLevel.Debug, "Socket Receive Completed with status: " + err);
                    if (err == SocketError.Success)
                    {
                        return state.buffer.Take(readBytes).ToArray();
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.TimedOut) {
                    _log.LogError(ex, "Socket Receive Error", new { ex.SocketErrorCode  }); 
                    throw;
                }
            }
            return null;
        }

    }
}