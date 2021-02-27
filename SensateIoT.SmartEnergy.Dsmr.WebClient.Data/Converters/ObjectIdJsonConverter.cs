﻿using System;
using System.Collections.Generic;

using MongoDB.Bson;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Converters
{
	public class ObjectIdJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type type)
		{
			return type == typeof(ObjectId);
		}

		public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
		{
			var ids = new List<ObjectId>();
			var token = JToken.Load(reader);
			ObjectId id;

			if (token.Type == JTokenType.Array)
			{
				foreach (var i in token.ToObject<string[]>())
				{
					if (!ObjectId.TryParse(i, out id))
						continue;

					ids.Add(id);
				}

				return ids.ToArray();
			}

			if (token.ToObject<string>().Equals("MongoDB.Bson.ObjectId[]"))
			{
				return ids.ToArray();
			}

			if (ObjectId.TryParse(token.ToObject<string>(), out id))
				return id;

			return null;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value.GetType().IsArray)
			{
				writer.WriteStartArray();

				foreach (var item in (Array)value)
				{
					serializer.Serialize(writer, item);
				}

				writer.WriteEndArray();
			}
			else
			{
				serializer.Serialize(writer, value.ToString());
			}
		}
	}
}