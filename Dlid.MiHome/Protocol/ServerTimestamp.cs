using System;

namespace Dlid.MiHome.Protocol
{
    /// <summary>
    /// A container class to keep track of the latest timestamp received from the device and the time it was received
    /// </summary>
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
