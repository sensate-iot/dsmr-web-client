using System.Collections.Generic;
using SensateIoT.SmartEnergy.Dsmr.WebClientService.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings
{
	public class Listener
	{
		public string UserId { get; set; }
		public string ApiKey { get; set; }
		public IDictionary<string, Sensor> Sensors { get; }
		//public IDictionary<string, string> GasSensorMapping { get; }
		//public IDictionary<string, string> GasSensors { get; }

		public Listener()
		{
			this.Sensors = new Dictionary<string, Sensor>();
			//this.GasSensorMapping = new Dictionary<string, string>();
			//this.GasSensors = new Dictionary<string, string>();
		}
	}
}
