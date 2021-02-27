using System;
using System.Collections.Generic;
using MongoDB.Bson;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Data
{
	public class MeasurementBuilder
	{
		private readonly Measurement m_measurement;

		public MeasurementBuilder()
		{
			this.m_measurement = new Measurement();
		}

		public MeasurementBuilder WithCoordinates(double longitude, double latitude)
		{
			this.m_measurement.Longitude = longitude;
			this.m_measurement.Latitude = latitude;

			return this;
		}

		public MeasurementBuilder WithSecret(string secret)
		{
			this.m_measurement.Secret = secret;
			return this;
		}

		public MeasurementBuilder WithTimestamp(DateTime timestamp)
		{
			this.m_measurement.Timestamp = timestamp;
			return this;
		}

		public MeasurementBuilder WithSensorId(string sensorId)
		{
			this.m_measurement.SensorId = ObjectId.Parse(sensorId);
			return this;
		}

		public MeasurementBuilder WithDataPoint(string key, string unit, decimal value, double? accuracy = null)
		{
			var dp = new DataPoint {
				Value = value,
				Unit = unit,
				Accuracy = accuracy
			};

			if(this.m_measurement.Data == null) {
				this.m_measurement.Data = new Dictionary<string, DataPoint>();
			}

			this.m_measurement.Data.Add(new KeyValuePair<string, DataPoint>(key, dp));
			return this;
		}

		public Measurement Build()
		{
			return this.m_measurement;
		}
	}
}
