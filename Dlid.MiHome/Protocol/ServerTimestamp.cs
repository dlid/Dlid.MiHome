using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Protocol
{
    internal class ServerTimestamp
    {
        internal ServerTimestamp(long ts)
        {
            Timestamp = ts;
            ReceivedTime = DateTime.Now;
        }

        public DateTime ReceivedTime { get; set; }
        public long Timestamp { get; set; }
    }
}
