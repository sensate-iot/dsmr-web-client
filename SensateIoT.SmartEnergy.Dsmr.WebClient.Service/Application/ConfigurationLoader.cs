using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings;
using SensateIoT.SmartEnergy.Dsmr.WebClientService.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Application
{
	public static class ConfigurationLoader
	{
		public static AppSettings BuildAppSettings()
		{
			return new AppSettings {
				Remote = BuildRemoteSettings(),
				Listener = BuildListenerSettings()
			};
		}

		private static Listener BuildListenerSettings()
		{
			var listener = new Listener {
				UserId = ConfigurationManager.AppSettings["userId"] ??  throw new InvalidOperationException("No valid user ID configured!"),
				ApiKey = ConfigurationManager.AppSettings["apiKey"] ??  throw new InvalidOperationException("No valid API key configured!")
			};

			LoadSensors(listener);
			return listener;
		}

		private static void LoadSensors(Listener listener)
		{
			var sensorsFile = ConfigurationManager.AppSettings["sensorsFile"];

			if(string.IsNullOrEmpty(sensorsFile)) throw new InvalidOperationException("No settings file configured.");
			if(!File.Exists(sensorsFile)) throw new FileNotFoundException("Unable to open sensors file.", sensorsFile);

			var sensors = JsonConvert.DeserializeObject<IEnumerable<Sensor>>(File.ReadAllText(sensorsFile));

			foreach(Sensor sensor in sensors) {
				listener.Sensors[sensor.Id] = sensor;
			}
			/*foreach(DictionaryEntry sensor in sensors) {
				listener.Sensors[sensor.Key.ToString()] = sensor.Value.ToString();
			}

			foreach(DictionaryEntry gasSensor in gasSensors) {
				var originSensor = gasSensor.Key.ToString();
				var targetKeyValue = gasSensor.Value.ToString().Split(',');

				listener.GasSensorMapping[originSensor] = targetKeyValue[0];
				listener.GasSensors[targetKeyValue[0]] = targetKeyValue[1];

				if(!listener.Sensors.ContainsKey(originSensor)) {
					throw new InvalidOperationException($"Invalid gas sensor mapping. Cannot map {originSensor} to {targetKeyValue[0]}");
				}
			}*/
		}

		private static Remote BuildRemoteSettings()
		{
			var portString = ConfigurationManager.AppSettings["port"] ?? "80";
			var port = Convert.ToUInt16(portString);

			var remote = new Remote {
				Path = "/live/v1/control",
				Port = port,
				Host = ConfigurationManager.AppSettings["hostname"],
				Scheme = port == 443 ? "wss" : "ws",
				storageUri = new Uri(ConfigurationManager.AppSettings["storageUri"])
			};

			return remote;
		}
	}
}
