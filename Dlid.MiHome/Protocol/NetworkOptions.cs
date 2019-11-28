using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    /// <summary>
    /// Various options for communicating with the device
    /// </summary>
    public class NetworkOptions
    {

        private TimeSpan _sendTimeout = new TimeSpan(0, 0, 0, 1);
        private TimeSpan _receiveTimeout = new TimeSpan(0, 0, 0, 1);
        private int _retryCount = 5;
        private TimeSpan _retryDelay = new TimeSpan(0, 0, 0, 0, 500);
        private TimeSpan _handshakeEvery = new TimeSpan(0, 2, 0);
        private int _networkPort = 54321;

        /// <summary>
        /// The time to wait while sending a socket request before timing out
        /// </summary>
        public TimeSpan SendTimeout {
            get => _sendTimeout;
            set => _sendTimeout = value == null ? new TimeSpan(0) : value;
        } 

        /// <summary>
        /// The time to wait for a response before timing out
        /// </summary>
        public TimeSpan ReceiveTimeout {
            get => _receiveTimeout;
            set => _receiveTimeout = value == null ? new TimeSpan(0) : value;
        } 

        /// <summary>
        /// The number of times to retry a command before throwing an error
        /// </summary>
        public int RetryCount { 
            get => _retryCount;
            set => _retryCount = value < 0 ? 0 : value;
        }
        
        /// <summary>
        /// If a command fails/times out - wait this amount of time before trying again
        /// </summary>
        public TimeSpan RetryDelay {
            get => _retryDelay;
            set => _retryDelay = value == null ? new TimeSpan(0) : value;
        }

        /// <summary>
        /// A handshake will be sent before every command if one has not been received for this amount of time
        /// </summary>
        public TimeSpan HandshakeEvery {
            get => _handshakeEvery;
            set => _handshakeEvery = value == null || value.TotalSeconds < 5 ? new TimeSpan(0, 0, 5) : value;
        }

        /// <summary>
        /// The network port to use 
        /// </summary>
        public int NetworkPort
        {
            get => _networkPort;
            set => _networkPort = value;
        }

    }
}
