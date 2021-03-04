using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClientService.Settings
{
	public class Sensor
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("key")]
		public string Key { get; set; }
		[JsonProperty("gasSensor")]
		public DsmrSensor GasSensor { get; set; }
		[JsonProperty("powerSensor")]
		public DsmrSensor PowerSensor { get; set; }
	}

	public class DsmrSensor 
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("key")]
		public string Key { get; set; }
	}
}
