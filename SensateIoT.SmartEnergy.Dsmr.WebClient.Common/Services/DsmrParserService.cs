using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

using log4net;
using Newtonsoft.Json;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Converters;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Settings;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.WCF.Parser;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public sealed class DsmrParserService : IWebSocketEventService
	{
		private readonly ILog m_logger;
		private ParserServiceClient m_parserClient;
		private readonly IMeasurementStorageService m_storageService;
		private readonly ParserSettings m_settings;

		private const string EndpointConfigName = "WSHttpBinding_IParserService";

		public DsmrParserService(IMeasurementStorageService storageService, ParserSettings settings, ILog logger)
		{
			this.m_logger = logger;
			this.m_parserClient = new ParserServiceClient(EndpointConfigName);
			this.m_storageService = storageService;
			this.m_settings = settings;
		}

		public void HandleWebSocketEvent(object origin, WebSocketEventArgs args)
		{
			switch(args.Type) {
				case EventType.Rx:
					this.m_logger.Info("Received DSMR message.");
					var parsed = this.ParseControlMessages(args.Data).ToList();
					var removed = parsed.RemoveAll(t => t == null);
					this.m_logger.Info($"Writing {parsed.Count} measurements to Sensate IoT. {removed} have " +
					                   "been discarded due to parsing issues.");

					var measurements = parsed.Select(this.buildElectricalMeasurement).ToList();
					measurements.AddRange(parsed.Select(this.buildGasMeasurement));
					this.postMeasurements(measurements).GetAwaiter().GetResult();

					break;

				case EventType.Connected:
				case EventType.Ping:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private Task postMeasurements(IEnumerable<Measurement> measurements)
		{
			var tasks = new List<Task>();

			foreach(var measurement in measurements) {
				tasks.Add(this.m_storageService.StoreAsync(measurement));
			}

			return Task.WhenAll(tasks);
		}

		private Measurement buildGasMeasurement(Telegram telegram)
		{
			if(telegram.GasData is null) {
				return null;
			}

			var builder = this.createMeasurementBuilder(telegram);
			var sensor = this.m_settings.Sensors[telegram.SensorId];

			if(sensor.GasSensor == null) {
				return null;
			}

			builder.WithSensorId(sensor.GasSensor.Id);
			builder.WithSecret(sensor.GasSensor.Key);
			builder.WithTimestamp(telegram.Timestamp);
			builder.WithDataPoint(nameof(Telegram.GasData.GasConsumption), "m3", telegram.GasData.GasConsumption, 0.001);
			builder.WithDataPoint(nameof(Telegram.GasData.GasFlow), "m3/min", telegram.GasData.GasFlow, 0.001);

			return builder.Build();
		}

		private Measurement buildElectricalMeasurement(Telegram telegram)
		{
			var builder = this.createMeasurementBuilder(telegram);
			var sensor = this.m_settings.Sensors[telegram.SensorId];

			builder.WithSecret(sensor.PowerSensor.Key);
			builder.WithSensorId(sensor.PowerSensor.Id);
			builder.WithTimestamp(telegram.Timestamp);

			builder.WithDataPoint("Tariff", "", telegram.CurrentTariff == "NORMAL" ? 1 : 0)
				.WithDataPoint(nameof(Telegram.PowerData.InstantaneousVoltage), "V", telegram.PowerData.InstantaneousVoltage, 0.1)
				.WithDataPoint(nameof(Telegram.PowerData.InstantaneousCurrent), "A", telegram.PowerData.InstantaneousCurrent, 0.1)
				.WithDataPoint(nameof(Telegram.PowerData.InstantaneousPowerProduction), "W", telegram.PowerData.InstantaneousPowerProduction * 1000, 1.0)
				.WithDataPoint(nameof(Telegram.PowerData.InstantaneousPowerUsage), "W", telegram.PowerData.InstantaneousPowerUsage * 1000, 1.0);

			builder.WithDataPoint(nameof(Telegram.EnergyData.EnergyConsumptionTariff1), "kWh", telegram.EnergyData.EnergyConsumptionTariff1, 0.001)
				.WithDataPoint(nameof(Telegram.EnergyData.EnergyProductionTariff1), "kWh", telegram.EnergyData.EnergyProductionTariff1, 0.001)
				.WithDataPoint(nameof(Telegram.EnergyData.EnergyConsumptionTariff2), "kWh", telegram.EnergyData.EnergyConsumptionTariff2, 0.001)
				.WithDataPoint(nameof(Telegram.EnergyData.EnergyProductionTariff2), "kWh", telegram.EnergyData.EnergyProductionTariff2, 0.001);

			return builder.Build();
		}

		private MeasurementBuilder createMeasurementBuilder(Telegram telegram)
		{
			var builder = new MeasurementBuilder();

			if(telegram.Latitude.HasValue && telegram.Longitude.HasValue) {
				builder.WithCoordinates(telegram.Longitude.Value, telegram.Latitude.Value);
			}

			builder.WithTimestamp(DateTime.UtcNow);

			return builder;
		}

		private IEnumerable<Telegram> ParseControlMessages(string data)
		{
			IEnumerable<Telegram> telegrams = null;

			try {
				var messages = JsonConvert.DeserializeObject<BulkControlMessage>(data);
				var textTelegrams = messages.Messages.Select(DsmrTelegramConverter.Convert).ToList();
					
				telegrams = textTelegrams.Select(textTelegram => this.ParseTelegram(messages.SensorId, textTelegram)).ToList();

				this.m_logger.Debug($"Publishing measurements: {JsonConvert.SerializeObject(telegrams)}");
			} catch(JsonSerializationException ex) {
				this.m_logger.Error($"Unable to parse control message! Message: {data}", ex);
			} catch(InvalidOperationException ex) {
				this.m_logger.Error($"Unable to parse text telegrams. Telegram data: {data}.", ex);
			}

			return telegrams;
		}

		private Telegram ParseTelegram(string sensorId, TextTelegram textTelegram)
		{
			Telegram telegram = null;

			try {
				this.m_logger.Info($"Origin sensor: {sensorId}.");

				telegram = this.m_parserClient.Parse(textTelegram.Telegram);
				telegram.SensorId = sensorId;
				telegram.Timestamp = textTelegram.Timestamp;

				if(textTelegram.Longitude > 0D && textTelegram.Latitude > 0D) {
					telegram.Latitude = textTelegram.Latitude;
					telegram.Longitude = textTelegram.Longitude;
				}
			} catch(ServerTooBusyException ex) {
				this.m_logger.Error("DSMR parser timeout. Is the remote up?", ex);
				this.m_logger.Error($"Telegram text: {textTelegram.Telegram}");
			} catch(FaultException ex) {
				this.m_logger.Error("Error in the DSMR parser service.", ex);
				this.m_logger.Error($"Telegram text: {textTelegram.Telegram}");
			} catch(CommunicationException ex) {
				this.m_logger.Error("Unknown DSMR parsing exception.", ex);
				this.m_logger.Error($"Telegram text: {textTelegram.Telegram}");

				if(this.m_parserClient.State == CommunicationState.Faulted) {
					this.m_parserClient = new ParserServiceClient(EndpointConfigName);
				}
			}

			return telegram;
		}

		public void Dispose()
		{
			this.m_storageService.Dispose();
		}
	}
}
