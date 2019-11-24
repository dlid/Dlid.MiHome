using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    public class NetworkOptions
    {
        public TimeSpan SendTimeout { get; set; } = new TimeSpan(0, 0, 0, 1);
        public TimeSpan ReceiveTimeout { get; set; } = new TimeSpan(0, 0, 0, 1);
        public int RetryCount { get; set; } = 5;
        public TimeSpan RetryDelay { get; set; } = new TimeSpan(0, 0, 0, 0, 500);
    }
}
