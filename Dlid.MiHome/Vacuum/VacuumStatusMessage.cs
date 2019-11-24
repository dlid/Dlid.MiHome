
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Vacuum
{
    public class VacuumStatusMessage
    {

        /// <summary>
        /// Battery level
        /// </summary>
        [JsonProperty("battery")]
        public int Battery { get; set; }

        /// <summary>
        /// Total Area (in cm2)
        /// </summary>
        [JsonProperty("clean_area")]
        public int CleanArea { get; set; }

        /// <summary>
        /// Total cleaning time
        /// </summary>
        [JsonProperty("clean_time")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan CleanTime { get; set; }

        /// <summary>
        /// Is Do Not Disturb enabled
        /// </summary>
        [JsonProperty("dnd_enabled")]
        public int DoNotDisturbEnabled { get; set; }

        /// <summary>
        /// Error Code (0 = No error)
        /// </summary>
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error Code (0 = No error)
        /// </summary>
        [JsonProperty("fan_power")]
        public int FanPower { get; set; }

        /// <summary>
        /// True if device is currently cleaning
        /// </summary>
        [JsonProperty("in_cleaning")]
        public bool InCleaning { get; set; }

        /// <summary>
        ///Unknown
        /// </summary>
        [JsonProperty("in_fresh_state")]
        public bool InFreshState { get; set; }

        /// <summary>
        /// Is returning to dock
        /// </summary>
        [JsonProperty("in_returning")]
        public bool InReturning { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        [JsonProperty("lab_status")]
        public int LabStatus { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        [JsonProperty("lock_status")]
        public int LockStatus { get; set; }

        /// <summary>
        /// Is map present
        /// </summary>
        [JsonProperty("map_present")]
        public bool MapPresent { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        [JsonProperty("map_status")]
        public int MapStatus { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        [JsonProperty("msg_seq")]
        public int MessageSequence { get; set; }

        /// <summary>
        /// Message version (seems always 4 and 2 for s6)
        /// </summary>
        [JsonProperty("msg_ver")]
        public int MessageVersion { get; set; }

        /// <summary>
        /// Message version (seems always 4 and 2 for s6)
        /// </summary>
        [JsonProperty("state")]
        public VacuumState State { get; set; }

    }
}
