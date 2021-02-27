using System.Collections.Generic;
using SensateIoT.SmartEnergy.Dsmr.WebClientService.Settings;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Settings
{
	public class ParserSettings
	{
		public string ApiKey { get; set; }
		public IDictionary<string, Sensor> Sensors { get; set; }
	}
}
