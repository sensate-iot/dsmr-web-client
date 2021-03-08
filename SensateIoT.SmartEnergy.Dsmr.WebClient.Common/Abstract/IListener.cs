using System.Threading;
using System.Threading.Tasks;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract
{
	public interface IListener
	{
		event BaseListener.WebSocketEventHandler OnWebSocketMessage;

		Task StartAsync(CancellationToken token);
		Task StopAsync();
		void AddSensor(string sensorId, string secret);
		void SetUserData(string userId, string apikey);
		Task AuthorizeSensorsAsync(CancellationToken ct);
		Task AuthorizeUserAsync(CancellationToken ct);
		Task PingAsync(CancellationToken ct);
	}
}
