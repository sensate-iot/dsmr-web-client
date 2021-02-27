using System;
using System.Text;

using Newtonsoft.Json.Linq;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Converters
{
	public static class TextTelegramConverter
	{
		public static TextTelegram Convert(ControlMessage message)
		{
			var telegram = new TextTelegram();
			var token = JToken.Parse(message.Data);

			if(token["latitude"] != null && token["longitude"] != null) {
				telegram.Latitude = token["latitude"].ToObject<double>();
				telegram.Longitude = token["longitude"].ToObject<double>();
			} else {
				telegram.Longitude = telegram.Latitude = 0D;
			}

			if(token["telegram"] == null) {
				throw new InvalidOperationException("Unable to parse telegram without a telegram.");
			}

			telegram.Timestamp = token["timestamp"].ToObject<DateTime>();
			var base64 = token["telegram"].ToString();
			var bytes  = System.Convert.FromBase64String(base64);

			telegram.Telegram = Encoding.UTF8.GetString(bytes);

			return telegram;
		}
	}
}
