using System;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public class AuthorizationService : IAuthorizationService
	{
		public string SignMeasurement(Measurement measurement)
		{
			var json = JsonConvert.SerializeObject(measurement);
			var hash = this.GenerateSha256Signature(json);

			return json.Replace(measurement.Secret, $"${hash}==");
		}

		public WebSocketRequest<SensorAuthorizationRequest> GenerateWebSocketSignature(string sensorId, string secret)
		{
			var request = new WebSocketRequest<SensorAuthorizationRequest> {
				Request = "subscribe",
				Data = new SensorAuthorizationRequest {
					Secret = secret,
					SensorId = sensorId,
					Timestamp = DateTime.UtcNow
				}
			};

			var json = JsonConvert.SerializeObject(request.Data);
			request.Data.Secret = this.GenerateSha256Signature(json);

			return request;
		}

		private string GenerateSha256Signature(string input)
		{
			string result;

			using(var sha = SHA256.Create()) {
				var bytes = sha.ComputeHash(Encoding.ASCII.GetBytes(input));
				var sb = new StringBuilder();

				foreach(var b in bytes) {
					sb.Append(b.ToString("x2"));
				}

				result = sb.ToString();
			}

			return result;
		}
	}
}
