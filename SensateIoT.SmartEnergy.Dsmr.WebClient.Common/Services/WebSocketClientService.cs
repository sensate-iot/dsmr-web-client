using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Abstract;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Events;
using SensateIoT.SmartEnergy.Dsmr.WebClient.Data.DTO;

namespace SensateIoT.SmartEnergy.Dsmr.WebClient.Common.Services
{
	public sealed class WebSocketClientService : IWebSocketClientService, IDisposable
	{
		public delegate void WebSocketEventHandler(object sender, WebSocketEventArgs e);
		public event WebSocketEventHandler OnWebSocketMessage;

		private static ILog logger = LogManager.GetLogger("WebSocketClientService");

		private string m_userId;
		private string m_apiKey;
		private readonly ClientWebSocket m_socket;
		private CancellationTokenSource m_ct;
		private Task m_backgroundTask;
		private readonly IList<Tuple<string, string>> m_sensors;
		private readonly ILog m_logger;
		private readonly Uri m_remote;

		public WebSocketClientService(Uri remote, ILog logger)
		{
			this.m_socket = new ClientWebSocket();
			this.m_backgroundTask = Task.CompletedTask;
			this.m_sensors = new List<Tuple<string, string>>();
			this.m_logger = logger;
			this.m_remote = remote;
		}

		public void AddSensor(string sensorId, string secret)
		{
			this.m_sensors.Add(new Tuple<string, string>(sensorId, secret));
			this.m_logger.Info($"Adding sensor: {sensorId}.");
		}

		public async Task AuthorizeSensorsAsync(CancellationToken ct)
		{
			var authService = new AuthorizationService();

			foreach(var (sensorId, secret) in this.m_sensors) {
				var subscribe = authService.GenerateWebSocketSignature(sensorId, secret);
				var json = JsonConvert.SerializeObject(subscribe);

				this.m_logger.Info($"Authorizing sensor: {sensorId}.");
				this.m_logger.Debug($"Authorization message: {json}");

				var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
				await this.m_socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
			}
		}

		public Task StartAsync(CancellationToken token)
		{
			this.m_ct = CancellationTokenSource.CreateLinkedTokenSource(token);
			this.m_backgroundTask = this.ExecuteInternalAsync(token);

			return this.m_backgroundTask;
		}

		public void SetUserData(string userId, string apikey)
		{
			this.m_userId = userId;
			this.m_apiKey = apikey;
		}

		public async Task StopAsync()
		{
			this.m_ct.Cancel();
			await this.m_backgroundTask.ConfigureAwait(false);
		}


		public async Task PingAsync(CancellationToken ct)
		{
			var request = new WebSocketRequest<PingRequest> {
				Request = "keepalive",
				Data = new PingRequest {Ping = "pong"}
			};


			var json = JsonConvert.SerializeObject(request);
			logger.Info("Writing PING request.");
			var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
			await this.m_socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
		}

		public async Task AuthorizeUserAsync(CancellationToken ct)
		{
			var request = new WebSocketRequest<AuthorizationRequest> {
				Request = "auth-apikey",
				Data = new AuthorizationRequest {ApiKey = this.m_apiKey, UserId = this.m_userId}
			};

			var json = JsonConvert.SerializeObject(request);

			logger.Debug($"Authorizing user: {json}");
			var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
			await this.m_socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
		}

		private void Invoke(string data, EventType type)
		{
			this.OnWebSocketMessage?.Invoke(this, new WebSocketEventArgs {
				Data = data,
				Type = type
			});
		}

		private async Task ExecuteInternalAsync(CancellationToken ct)
		{
			do {
				await this.m_socket.ConnectAsync(this.m_remote, ct).ConfigureAwait(false);
				this.Invoke(null, EventType.Connected);
				await ReceiveAsync(ct).ConfigureAwait(false);
			} while(!ct.IsCancellationRequested);
		}

		private async Task ReceiveAsync(CancellationToken ct)
		{
			var buffer = new ArraySegment<byte>(new byte[10000]);

			do {
				WebSocketReceiveResult result;

				using(var ms = new MemoryStream()) {
					do {
						result = await this.m_socket.ReceiveAsync(buffer, ct);
						if(buffer.Array == null) throw new InvalidOperationException("Buffer array is NULL.");
						ms.Write(buffer.Array, buffer.Offset, result.Count);
					} while(!result.EndOfMessage);

					if(result.MessageType == WebSocketMessageType.Close) {
						break;
					}

					var data = Encoding.UTF8.GetString(ms.ToArray());
					logger.Info($"Received data: {data}.");
					this.ParseRxEvent(data);
				}
			} while(!ct.IsCancellationRequested);
		}

		private void ParseRxEvent(string data)
		{
			var token = JToken.Parse(data);

			if(token["ping"] == null) {
				this.Invoke(data, EventType.Rx);
			} else {
				this.Invoke(data, EventType.Ping);
			}
		}

		public void Dispose()
		{
			this.m_socket?.Dispose();
			this.m_ct?.Dispose();
		}
	}
}
