using System;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Service.Settings
{
	public class Remote
	{
		public string Scheme { get; set; }
		public string Host { get; set; }
		public ushort Port { get; set; }
		public string Path { get; set; }
		public Uri storageUri { get; set; }
	}
}
