using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class WebSocketRequest<TValue> where TValue : class
	{
		[JsonProperty("request")]
		public string Request { get; set; }
		[JsonProperty("data")]
		public TValue Data { get; set; }
	}
}
