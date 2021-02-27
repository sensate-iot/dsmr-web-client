using System;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class TextTelegram
	{
		public string Telegram { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
