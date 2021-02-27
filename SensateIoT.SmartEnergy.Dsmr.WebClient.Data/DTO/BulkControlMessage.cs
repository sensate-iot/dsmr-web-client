using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class BulkControlMessage
	{
		[JsonProperty("messages")]
		public IEnumerable<ControlMessage> Messages { get; set; }
		[JsonProperty("sensorId")]
		public string SensorId { get; set; }
		[JsonProperty("secret")]
		public string Secret { get; set; }
		[JsonProperty("timestamp")]
		public DateTime Timestamp { get; set; }
	}
}
