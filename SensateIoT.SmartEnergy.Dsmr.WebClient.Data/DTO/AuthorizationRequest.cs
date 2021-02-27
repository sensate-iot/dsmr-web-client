using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO
{
	public class AuthorizationRequest
	{
		[JsonProperty("user")]
		public string UserId { get; set; }
		[JsonProperty("apikey")]
		public string ApiKey { get; set; }
	}
}