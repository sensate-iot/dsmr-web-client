using System;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events
{
	public class WebSocketEventArgs : EventArgs
	{
		public EventType Type { get; set; }
		public string Data { get; set; }
	}
}
