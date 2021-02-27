using System;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Converters;

using DataPointMap = System.Collections.Generic.IDictionary<string, SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models.DataPoint>;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models
{
	public class Measurement
	{
		[JsonRequired, JsonConverter(typeof(ObjectIdJsonConverter)), JsonProperty("sensorId")]
		public ObjectId SensorId { get; set; }
		[JsonRequired, JsonProperty("secret")]
		public string Secret { get; set; }
		[JsonProperty("longitude", NullValueHandling = NullValueHandling.Ignore)]
		public double? Longitude { get; set; }
		[JsonProperty("latitude", NullValueHandling = NullValueHandling.Ignore)]
		public double? Latitude { get; set; }
		[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? Timestamp { get; set; }
		[JsonRequired, JsonProperty("data")]
		public DataPointMap Data { get; set; }
	}
}