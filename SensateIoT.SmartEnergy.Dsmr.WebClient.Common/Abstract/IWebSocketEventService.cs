using System;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract
{
	public interface IWebSocketEventService : IDisposable
	{
		void HandleWebSocketEvent(object origin, WebSocketEventArgs args);
	}
}
