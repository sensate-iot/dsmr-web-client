using System.Collections.Generic;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings
{
	public class Listener
	{
		public string UserId { get; set; }
		public string ApiKey { get; set; }
		public IDictionary<string, Sensor> Sensors { get; }

		public Listener()
		{
			this.Sensors = new Dictionary<string, Sensor>();
		}
	}
}
