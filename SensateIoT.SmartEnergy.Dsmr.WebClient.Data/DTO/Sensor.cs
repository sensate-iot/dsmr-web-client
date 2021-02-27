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
		public GasSensor GasSensor { get; set; }
	}

	public class GasSensor 
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("key")]
		public string Key { get; set; }
	}
}
