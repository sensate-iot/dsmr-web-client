using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.Models;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public sealed class MeasurementStorageService : IMeasurementStorageService
	{
		private readonly HttpClient m_client;
		private readonly Uri m_remote;
		private readonly AuthorizationService m_auth;
		private readonly ILog m_logger;
		private readonly string m_apiKey;

		public MeasurementStorageService(Uri remote, string key, ILog logger)
		{
			this.m_client = new HttpClient();
			this.m_remote = remote;
			this.m_logger = logger;
			this.m_auth = new AuthorizationService();
			this.m_apiKey = key;
		}

		public async Task StoreAsync(Measurement measurement, CancellationToken ct = default)
		{
			var json = this.m_auth.SignMeasurement(measurement);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			content.Headers.Add("X-ApiKey", this.m_apiKey);

			this.m_logger.Debug($"Writing JSON to Sensate IoT Gateway: {json}");
			var result = await this.m_client.PostAsync(this.m_remote, content, ct).ConfigureAwait(false);
			json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

			this.m_logger.Debug($"Sensate IoT Gateway response: {json}");
		}

		public void Dispose()
		{
			this.m_client.Dispose();
		}
	}
}