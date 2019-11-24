using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome
{
    class TimespanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeSpan) || objectType == typeof(int))
            {
                return true;
            }
            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TimeSpan)
            {
                var jo = new JValue(((TimeSpan)value).TotalSeconds);
                jo.WriteTo(writer);
            } 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JValue.Load(reader);

            int seconds;
            if (int.TryParse(jo.ToString(), out seconds))
            {
                return new TimeSpan(0, 0, seconds);
            }

            return new TimeSpan(0);
        }
    }
}
