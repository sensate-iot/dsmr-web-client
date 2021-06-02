using System;

using Newtonsoft.Json;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Converters
{
	public class DecimalJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(decimal) || objectType == typeof(decimal?);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch(reader.TokenType) {
			case JsonToken.String when ((string)reader.Value) == string.Empty:
				return decimal.MinValue;

			case JsonToken.Float:
			case JsonToken.Integer:
				return Convert.ToDecimal(reader.Value);

			default:
				throw new JsonSerializationException( $"Unexpected token type: {reader.TokenType.ToString()}" );
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = (decimal)value;

			if (v == decimal.MinValue) {
				writer.WriteValue(string.Empty);
			} else {
				writer.WriteValue(value);
			}
		}
	}
}
