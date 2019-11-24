using Dlid.MiHome.Protocol;
using Serilog;
using Serilog.Core;
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

    internal class NetworkConnection
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
//        private Packet _packet;
        private ILogger _logger = Log.ForContext<NetworkConnection>();

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
                    _socket.Close();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        public void Connect(string address, int port, string token)
        {
            //Console.WriteLine("Connecting to {0}:{1}", address, port);
            _socket.Connect(IPAddress.Parse(address), port);

        }

        public byte[] Send(byte[] data, NetworkOptions options)
        {
            SocketError err;

            _socket.SendTimeout = (int)options.SendTimeout.TotalMilliseconds;
            _socket.Send(data, 0, data.Length, SocketFlags.None, out err);
            _logger.Debug("Send Completed with status: " + err);

            _socket.ReceiveTimeout = (int)options.ReceiveTimeout.TotalMilliseconds;

            try
            {
                int readBytes = _socket.Receive(state.buffer, 0, bufSize, SocketFlags.None, out err);
                _logger.Debug("Receive Completed with status: " + err);

                if (err == SocketError.Success)
                {
                    //_packet.Raw = state.buffer.Take(readBytes).ToArray();
                    return state.buffer.Take(readBytes).ToArray();
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // tid ut
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

    }
}