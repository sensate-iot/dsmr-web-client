using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class PingRequest
	{
		[JsonProperty("ping")]
		public string Ping { get; set; }
	}
}