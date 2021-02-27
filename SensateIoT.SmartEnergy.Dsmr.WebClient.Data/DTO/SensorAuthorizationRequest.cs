using System;
using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class SensorAuthorizationRequest
	{
		[JsonProperty("sensorId")]
		public string SensorId { get; set; }
		[JsonProperty("sensorSecret")]
		public string Secret { get; set; }
		[JsonProperty("timestamp")]
		public DateTime Timestamp { get; set; }
	}
}
