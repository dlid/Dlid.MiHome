using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Newtonsoft.Json.Linq;
using Dlid.MiHome.Protocol;

namespace Dlid.MiHome
{
    public class MiDevice : IDisposable
    {
        private string _ipAddress;
        private string _token;
        internal MiHomeToken _miToken;
        internal NetworkConnection _socket;
        internal int _requestId = 0;
        internal byte[] _deviceId;

        public NetworkOptions NetworkOptions { get; set; } = new NetworkOptions();

        /// <summary>
        /// The server timestamp is received from the device responses
        /// In the requests we will use this value and add the number of seconds that has passed since we received the last response
        /// </summary>
        internal ServerTimestamp _serverTimestamp;

        public MiDevice(string IPAddress, string Token)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
                //.WriteTo.File(@".\logs\Dlid.SmaryHome.VauumLib.log",
                //    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {SourceContext}{Message:lj}{Exception}{NewLine}",
                //    fileSizeLimitBytes: 10240000,
                //    rollOnFileSizeLimit: true,
                //    flushToDiskInterval: TimeSpan.FromMinutes(5))
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {SourceContext}{Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
            .CreateLogger();

            _ipAddress = IPAddress;
            _token = Token;
            _miToken = new MiHomeToken(Token);
            
            Log.Logger.Debug($"Created Device {IPAddress} ");

            // Unique enough Id base
            _requestId = Guid.NewGuid().GetHashCode() / 2;
        }

        private void Connect()
        {
            if (_socket == null)
            {
                Log.Logger.Debug($"Connecting to {this._ipAddress}");
                _socket = new NetworkConnection();

                _socket.Connect(_ipAddress, 54321, _token);
            }
            if (InNeedOfHandshake)
            {
                var handshakeResponse = Handshake();
                if (!handshakeResponse.Success)
                {
                    throw new Exception("Handshake failed");
                }
            }
        }


        private MiHomeResponse Handshake()
        {
            var miRequest = new MiHomeRequest(NetworkOptions);
            return Send(miRequest);
        }

        private bool InNeedOfHandshake
        {
            get
            {
                //TODO: Check time and stuff-stuff!
                return _serverTimestamp == null;
            }
        }

        private void NextId()
        {
            if (_requestId + 100 > int.MaxValue) {
                _requestId = Guid.NewGuid().GetHashCode() / 2;
            } 
            _requestId += 100;
        }

        private MiHomeResponse Send(MiHomeRequest request)
        {
            if (!request.IsHandshake) { 
                Connect();
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
                    var miResponse = new MiHomeResponse(_miToken, responsePayload);
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
            } while (retryCount > 0);

            return new MiHomeResponse();
        }

        public MiHomeResponse Send(string methodName, params object[] parameters)
        {
            var miRequest = new MiHomeRequest(_miToken, _deviceId, _serverTimestamp, NetworkOptions, new
            {
                method = methodName,
                @params = parameters
            });
            return Send(miRequest);
        }

        public MiHomeResponse Send(object data)
        {
            var myRequest = new MiHomeRequest(_miToken, _deviceId, _serverTimestamp, NetworkOptions, data);
            return Send(myRequest);
        }


        public void Dispose()
        {
            if (this._socket != null)
            {
                this._socket.Close();
            }
        }
    }
}
