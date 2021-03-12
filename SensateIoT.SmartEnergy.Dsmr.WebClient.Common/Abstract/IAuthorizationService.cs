using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract
{
	public interface IAuthorizationService
	{ 
		string SignMeasurement(Measurement measurement);
		WebSocketRequest<SensorAuthorizationRequest> GenerateWebSocketSignature(string sensorId, string secret);
	}
}
