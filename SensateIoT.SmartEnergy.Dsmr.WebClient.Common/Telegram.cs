using System;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.WCF.Parser
{
	public partial class Telegram
	{
		public string SensorId { get; set; }
		public double? Longitude { get; set; }
		public double? Latitude { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
