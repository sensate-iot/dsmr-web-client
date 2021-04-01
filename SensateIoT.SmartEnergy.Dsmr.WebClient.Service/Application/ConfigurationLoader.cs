using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings;

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
				ApiKey = ConfigurationManager.AppSettings["apiKey"] ??  throw new InvalidOperationException("No valid API key configured!"),
			};

			if(TimeSpan.TryParse(ConfigurationManager.AppSettings["subscriptionInterval"], out var ts)) {
				listener.SubscriptionInterval = ts;
			}

			LoadSensors(listener);
			return listener;
		}

		private static void LoadSensors(Listener listener)
		{
			var sensorsFile = ConfigurationManager.AppSettings["sensorsFile"];

			if(string.IsNullOrEmpty(sensorsFile)) throw new InvalidOperationException("No settings file configured.");
			if(!File.Exists(sensorsFile)) throw new FileNotFoundException("Unable to open sensors file.", sensorsFile);

			var sensors = JsonConvert.DeserializeObject<IEnumerable<Sensor>>(File.ReadAllText(sensorsFile)).ToList();
			VerifySensorConfiguration(sensors);

			foreach(var sensor in sensors) {
				listener.Sensors[sensor.Id] = sensor;
			}
		}

		private static void VerifySensorConfiguration(IEnumerable<Sensor> sensors)
		{
			foreach(var sensor in sensors) {
				if(sensor.Id == null) throw new InvalidOperationException("Sensor ID missing!");
				if(sensor.PowerSensor == null) throw new InvalidOperationException($"Power sensor missing for sensor {sensor.Id}");
			}
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
				StorageUri = new Uri(ConfigurationManager.AppSettings["storageUri"])
			};

			return remote;
		}
	}
}
