using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Vacuum
{
    public class VacuumConsumables
    {
        [JsonProperty("main_brush_work_time")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan MainBrushWorkTime { get; set; }

        public TimeSpan MainBrushLifeSpan { get; set; } = new TimeSpan(300, 0, 0);

        [JsonProperty("side_brush_work_time")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan SideBrushWorkTime { get; set; }

        public TimeSpan SideBrushLifeSpan { get; set; } = new TimeSpan(200, 0, 0);

        [JsonProperty("filter_work_time")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan FilterWorkTime { get; set; }

        public TimeSpan FilterLifeSpan { get; set; } = new TimeSpan(150, 0, 0);

        [JsonProperty("sensor_dirty_time")]
        [JsonConverter(typeof(TimespanConverter))]
        public TimeSpan SensorDirtyTime { get; set; }

        public TimeSpan SensorCleanInterval { get; set; } = new TimeSpan(30, 0, 0);


        public double FilterRemainingPercentage
        {
            get
            {
                if (FilterLifeSpan != null && FilterWorkTime != null)
                {
                    var percentage = FilterWorkTime.TotalSeconds / FilterLifeSpan.TotalSeconds;
                    return Math.Round((1 - percentage) * 100);
                }
                return 0;
            }
        }

        public double MainBrushRemainingPercentage
        {
            get
            {
                if (MainBrushLifeSpan != null && MainBrushWorkTime != null)
                {
                    var percentage = MainBrushWorkTime.TotalSeconds / MainBrushLifeSpan.TotalSeconds;
                    return Math.Round((1 - percentage) * 100);
                }
                return 0;
            }
        }

        public double SideBrushRemainingPercentage
        {
            get
            {
                if (SideBrushLifeSpan != null && SideBrushWorkTime != null)
                {
                    var percentage = SideBrushWorkTime.TotalSeconds / SideBrushLifeSpan.TotalSeconds;
                    return Math.Round((1 - percentage) * 100);
                }
                return 0;
            }
        }

        public double SensorRemainingPercentage
        {
            get
            {
                if (SensorCleanInterval!= null && SensorDirtyTime != null)
                {
                    var percentage = SensorDirtyTime.TotalSeconds / SensorCleanInterval.TotalSeconds;
                    return Math.Round((1 - percentage) * 100);
                }
                return 0;
            }
        }

    }
}
