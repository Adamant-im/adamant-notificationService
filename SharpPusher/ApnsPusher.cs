using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Jose;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpPusher
{
	public class ApnsPusher: IPusher<ApnsNotification, ApnsResult>
	{
		#region Const

		public readonly string ProductionUrl = "https://api.push.apple.com";
		public readonly string SandboxUrl = "https://api.development.push.apple.com";

		public readonly JwsAlgorithm Algorithm = JwsAlgorithm.ES256;
		private readonly string AlgorithmAsString = "ES256";

		public event NotificationSuccessHandler<ApnsNotification> OnNotificationSuccess;
		public event NotificationFailedHandler<ApnsNotification, ApnsResult> OnNotificationFailed;

		private JsonSerializerSettings _jsonSettings;

		#endregion


		#region Properties

		public ApnsEnvironment Environment { get; }
		public string Host { get; private set; }

		public string KeyId { get; }
		public string TeamId { get; }
		public string BundleAppId { get; }

		public ECDsa PrivateKey { get; }

		public string JwtToken { get; private set; }
		public DateTime JwtTokenDate { get; private set; }

        private readonly ILogger<ApnsPusher> _logger;

        #endregion


        #region Ctor

        public ApnsPusher(ILogger<ApnsPusher> logger, string keyId, string teamId, string bundleAppId, string certificatePath, string certificatePassword, ApnsEnvironment environment, int port = 443)
		{
            _logger = logger;
			Environment = environment;
			KeyId = keyId;
			TeamId = teamId;
			BundleAppId = bundleAppId;

			switch (Environment)
			{
				case ApnsEnvironment.Production:
					Host = $"{ProductionUrl}:{port}/3/device/";
					break;

				case ApnsEnvironment.Sandbox:
					Host = $"{SandboxUrl}:{port}/3/device/";
					break;
			}

            try
            {
                var certificate = new X509Certificate2(certificatePath, certificatePassword);
                PrivateKey = certificate.GetECDsaPrivateKey();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Failed to load certeficate at {certificatePath} with password {certificatePassword}\n");
                throw;
            }

			if (PrivateKey == null)
				throw new ArgumentException("Certificate does not contains ECDsa private key.");

			_jsonSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				NullValueHandling = NullValueHandling.Ignore
			};
		}

        #endregion


        #region Logic

        private readonly object jwtLock = new object();

        /// <summary>
        /// Packet send
        /// </summary>
        public void SendNotificationsAsync(IEnumerable<(ApnsNotification notification, string token)> notifications) {
            lock (jwtLock)
            {
                if (DateTime.Now.Subtract(JwtTokenDate).TotalHours > 1)
                {
                    JwtToken = GenerateNewJwtToken();
                    JwtTokenDate = DateTime.Now;
                }
            }

            foreach (var pair in notifications)
            {
                Task.Run(() => SendNotificationInternal(pair.notification, pair.token));
            }
        }

        /// <summary>
        /// Send single notification
        /// </summary>
        public void SendNotificationAsync(ApnsNotification notification, string deviceToken)
        {
            lock (jwtLock)
            {
                if (DateTime.Now.Subtract(JwtTokenDate).TotalHours > 1)
                {
                    JwtToken = GenerateNewJwtToken();
                    JwtTokenDate = DateTime.Now;
                }
            }

            Task.Run(() => SendNotificationInternal(notification, deviceToken));
        }

        private async Task SendNotificationInternal(ApnsNotification notification, string deviceToken)
		{
			var payload = JsonConvert.SerializeObject(notification, _jsonSettings);
			var payloadBytes = new ByteArrayContent(Encoding.UTF8.GetBytes(payload));

			var uri = new Uri(Host + deviceToken);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);

            try {
                using (var client = new HttpClient())
				{
					// Prepare HTTP Request
					var request = new HttpRequestMessage(HttpMethod.Post, uri);
                    request.Headers.Add("apns-id", Guid.NewGuid().ToString());
                    request.Headers.Add("apns-expiration", "0");
                    request.Headers.Add("apns-priority", "10");
                    request.Headers.Add("apns-topic", BundleAppId);
                    request.Content = payloadBytes;

                    request.Version = new Version(2, 0);

                    lock (jwtLock) {
                        request.Headers.Add("authorization", $"bearer {JwtToken}");
                    }

                    _logger.LogDebug($"Push URI: {uri}\nPayload content: {payload}\nHeaders: {request.Headers.ToString()}");

                    // Send request
                    var response = await client.SendAsync(request);

                    _logger.LogDebug($"Response: {response.ToString()}");

					// Handle response
					var apnsResult = (ApnsResult)response.StatusCode;

					if (apnsResult == ApnsResult.Success)
					{
						var args = new NotificationSuccessEventArgs<ApnsNotification>(deviceToken, notification);
						OnNotificationSuccess?.Invoke(this, args);
					}
					else
					{
						var bodyRaw = await response.Content.ReadAsStringAsync();
						var body = JsonConvert.DeserializeObject<ApnsResponse>(bodyRaw);
						var args = new NotificationFailedEventArgs<ApnsNotification, ApnsResult>(deviceToken, notification, apnsResult, body.Reason, null);
						OnNotificationFailed?.Invoke(this, args);
					}
				}
			} catch (Exception e) {
				var args = new NotificationFailedEventArgs<ApnsNotification, ApnsResult>(deviceToken, notification, ApnsResult.UnknownError, null, e);
				OnNotificationFailed?.Invoke(this, args);
			}
		}

		#endregion


		#region Tools

		/// <summary>
		/// Convert DateTime to Unix epoch.
		/// </summary>
		private long ToUnixEpochDate(DateTime date)
		{
			return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
		}

		private string GenerateNewJwtToken()
		{
			var payload = new Dictionary<string, object>
			{
				{ "iss", TeamId },
				{ "iat", ToUnixEpochDate(DateTime.UtcNow) }
			};
			var header = new Dictionary<string, object>
			{
				{ "alg", AlgorithmAsString },
				{ "kid", KeyId }
			};

			_logger.LogDebug("Renewed JWT token");

			return JWT.Encode(payload, PrivateKey, Algorithm, header);

        }

        #endregion
    }
}
