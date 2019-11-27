using Dlid.MiHome.Vacuum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dlid.MiHome
{
    public class MiVacuumDevice : MiDevice
    {
        public MiVacuumDevice(string IpAddress, string Token) : base(IpAddress, Token) {}

        public bool Start()
        {
            var status = GetStatus();
            if (status.State == VacuumState.SpotCleaning || status.State == VacuumState.Docking || status.State == VacuumState.ZoneCleaning)
            {
                Stop();
            } else if (status.State == VacuumState.Cleaning)
            {
                return true;
            }

            var response = Send("app_start");
            return response.Success;
        }

        public bool Stop()
        {
            var response = Send("app_stop");
            return response.Success;
        }

        public VacuumStatusMessage GetStatus()
        {
            var result = Send("get_status");
            if (result.Success && !string.IsNullOrEmpty(result.ResponseText))
            {
                JObject o = JObject.Parse(result.ResponseText);
                var yao = o.SelectToken("$.result[0]").ToObject<VacuumStatusMessage>();
                return yao;
            }
            return null;
        }

        public void Charge()
        {

            var currentStatus = GetStatus();

            if (currentStatus.InReturning)
            {
                return;
            }

            if (currentStatus.InCleaning)
            {
                Stop();
            }

            var result = Send("app_charge");
            if (result.Success && !string.IsNullOrEmpty(result.ResponseText))
            {
                JObject o = JObject.Parse(result.ResponseText);

              //  return yao;
            }
        }


        public string ZonedClean(int x1, int y1, int x2, int y2, int times)
        {
            return ZonedClean(new ZoneCleanParameters { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, NumberOfTimes = times });

        }

        public string ZonedClean(List<List<int>> coordinates)
        {
            return ZonedClean( coordinates.Select(c => new ZoneCleanParameters { X1 = c[0], X2 = c[1], Y1 = c[2], Y2 = c[3], NumberOfTimes = c[4] }).ToArray() );

        }

        public string ZonedClean(params ZoneCleanParameters[] param)
        {
            var result = Send("app_zoned_clean", new object[] {
                param.Select(zone => new object[] {
                    zone.X1,
                    zone.Y1,
                    zone.X2,
                    zone.Y2,
                    zone.NumberOfTimes
                })
            });
            if (result.Success && !string.IsNullOrEmpty(result.ResponseText))
            {
                //JObject o = JObject.Parse(result);
                //var yao = o.SelectToken("$.result[0]").ToObject<VacuumStatusMessage>();
                //return yao;
            }
            /*
             * 
             {
	"result": [{
		"main_brush_work_time": 172494,  // seconds change after 300h
		"side_brush_work_time": 172494,  // seconds change after 200h
		"filter_work_time": 172494,  // seconds change after 150h
		"sensor_dirty_time": 172488  // seconds clean after 30h
	}],
	"id": 101
}
 * 
 * */
            return null;
        }
        public VacuumConsumables GetConsumables()
        {
            var result = Send("get_consumable");
            if ( result.Success && !string.IsNullOrEmpty(result.ResponseText))
            {
                return result.As<VacuumConsumables>("$.result[0]");
            }
            return null;
        }

        private int TryGetInt(JObject obj, string path)
        {
            var o = obj.SelectToken(path);
            if (o != null && o is JValue)
            {
                int i;
                if (int.TryParse(((JValue)o).Value.ToString(), out i))
                {
                    return i;
                }
            }
            return 0;
        }

        private TimeSpan TryGetTimeSpanFromSeconds(JObject obj, string path)
        {
            return new TimeSpan(0, 0, TryGetInt(obj, path));
        }

        private long? TryGetLong(JObject obj, string path)
        {
            var o = obj.SelectToken(path);
            if (o != null && o is JValue)
            {
                long i;
                if (long.TryParse(((JValue)o).Value.ToString(), out i))
                {
                    return i;
                }
            }
            return null;
        }

        public VacuumStatusMessage GetCleanSummary()
        {

            var result = Send("get_clean_summary");
            if (result.Success && !string.IsNullOrEmpty(result.ResponseText))
            {
                JObject o = JObject.Parse(result.ResponseText);
                var totalCleaningTime = TryGetTimeSpanFromSeconds(o, "$.result[0]");
                var totalArea = TryGetLong(o, "$.result[1]");
                var numCleanups = TryGetInt(o, "$.result[2]");
            }
            return null;
        }
    }
}
