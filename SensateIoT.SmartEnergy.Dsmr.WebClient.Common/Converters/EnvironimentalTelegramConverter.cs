using System;
using System.Text;

using Newtonsoft.Json.Linq;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Converters
{
	public static class EnvironimentalTelegramConverter
	{
		private const string Needle = "/Environment";

		public static Measurement Convert(ControlMessage message)
		{
			var builder = new MeasurementBuilder();
			var token = JToken.Parse(message.Data);

			if(token["latitude"] != null && token["longitude"] != null) {
				var lat = token["latitude"].ToObject<double>();
				var lon = token["longitude"].ToObject<double>();

				builder.WithCoordinates(lon, lat);
			}

			if(token["telegram"] == null) {
				throw new InvalidOperationException("Unable to parse telegram without a telegram.");
			}

			builder.WithTimestamp(token["timestamp"].ToObject<DateTime>());
			builder.WithSensorId(message.SensorId);

			var base64 = token["telegram"].ToString();
			var bytes  = System.Convert.FromBase64String(base64);

			var tmp = Encoding.UTF8.GetString(bytes);
			tmp = tmp.Replace(Needle, "");
			var data = JToken.Parse(tmp);

			builder.WithDataPoint("pressure", "Pa", data["p"].ToObject<decimal>(), 0.1);
			builder.WithDataPoint("temperature", "C", data["t"].ToObject<decimal>(), 0.1);
			builder.WithDataPoint("rh", "%", data["rh"].ToObject<decimal>(), 0.00001);

			return builder.Build();
		}
	}
}
