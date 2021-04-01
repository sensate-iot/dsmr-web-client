using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class SensorUnsubscribeRequest
	{
		[JsonProperty("sensorId")]
		public string SensorId { get; set; }
	}
}
