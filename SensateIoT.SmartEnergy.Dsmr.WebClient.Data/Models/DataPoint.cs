using Newtonsoft.Json;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Converters;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models
{
	public class DataPoint
	{
		[JsonProperty("unit")]
		public string Unit { get; set; }
		[JsonProperty("value"), JsonConverter(typeof(DecimalJsonConverter)), JsonRequired]
		public decimal Value { get; set; }
		[JsonProperty("precision", NullValueHandling = NullValueHandling.Ignore)]
		public double? Precision { get; set; }
		[JsonProperty("accuracy", NullValueHandling = NullValueHandling.Ignore)]
		public double? Accuracy { get; set; }

		public DataPoint()
		{ }
	}
}
