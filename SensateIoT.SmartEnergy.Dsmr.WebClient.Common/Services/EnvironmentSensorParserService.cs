using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using log4net;
using Newtonsoft.Json;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Converters;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Settings;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public class EnvironmentSensorParserService : IWebSocketEventService
	{
		private readonly ILog m_logger;
		private readonly IMeasurementStorageService m_storageService;
		private readonly IDictionary<string, string> m_sensorKeyMap;

		public EnvironmentSensorParserService(IMeasurementStorageService storageService, ParserSettings settings, ILog logger)
		{
			this.m_logger = logger;
			this.m_storageService = storageService;
			this.m_sensorKeyMap = new Dictionary<string, string>();

			this.BuildSensorKeyMap(settings);
		}


		public void HandleWebSocketEvent(object origin, WebSocketEventArgs args)
		{
			if(args.Type != EventType.Rx) {
				return;
			}

			var messages = JsonConvert.DeserializeObject<BulkControlMessage>(args.Data);
			this.PostMeasurements(messages.Messages.ToList(), messages.SensorId).GetAwaiter().GetResult();
		}

		private void BuildSensorKeyMap(ParserSettings settings)
		{
			foreach(var kvp in settings.Sensors) {
				if(kvp.Value.EnvironmentSensor is null) {
					continue;
				}

				var id = kvp.Value.EnvironmentSensor.Id;
				this.m_sensorKeyMap[id] = kvp.Value.EnvironmentSensor.Key;
			}
		}

		private Task PostMeasurements(ICollection<ControlMessage> messages, string sensorId)
		{
			var tasks = new List<Task>();

			this.m_logger.Info($"Writing {messages.Count} environmental messages to the data broker.");
			var key = this.m_sensorKeyMap[sensorId];

			foreach(var msg in messages) {
				var m = EnvironimentalTelegramConverter.Convert(msg);
				m.Secret = key;
				var t = this.m_storageService.StoreAsync(m);

				tasks.Add(t);
			}

			return Task.WhenAll(tasks);
		}

		public void Dispose()
		{
			this.m_storageService.Dispose();
		}
	}
}
