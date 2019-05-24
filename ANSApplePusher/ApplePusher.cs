using System;
using System.Collections.Generic;
using Adamant.Models;
using Adamant.NotificationService.Models;
using SharpPusher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Adamant.NotificationService.ApplePusher
{
	public class ApplePusher: IPusher
	{
		#region Properties

		private readonly ILogger<ApplePusher> _logger;
		private readonly IConfiguration _configuration;

		private ApnsPusher pusher;
		private Dictionary<TransactionType, PayloadContent> _contents;

		public event InvalidTokenHandler OnInvalidToken;

		#endregion


		#region Ctor

		public ApplePusher(ILogger<ApplePusher> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;

			Func<string, IConfiguration, string> getRequiredParam = (key, config) =>
			{
				var value = config[key];
				if (value == null)
					throw new ArgumentException($"Can't get {key}");
				
				return value;
			};

			_contents = LoadPayloadContent(_configuration);

			var keyId = getRequiredParam("ApplePusher:Keys:keyId", configuration);
			var teamId = getRequiredParam("ApplePusher:Keys:teamId", configuration);
			var bundleAppId = getRequiredParam("ApplePusher:Keys:bundleAppId", configuration);
			var pfxPath = Utilities.HandleUnixHomeDirectory(getRequiredParam("ApplePusher:Keys:pfxPath", configuration));
			var pfxPassword = getRequiredParam("ApplePusher:Keys:pfxPassword", configuration);


			pusher = new ApnsPusher(keyId, teamId, bundleAppId, pfxPath, pfxPassword, ApnsEnvironment.Production);
			pusher.OnNotificationSuccess += Pusher_OnNotificationSuccess;
			pusher.OnNotificationFailed += Pusher_OnNotificationFailed;
		}

		#endregion


		public void Start()
		{
		}

		public void Stop()
		{
		}
		
		public void NotifyDevice(Device device, IEnumerable<Transaction> transactions)
		{
			if (device == null)
				throw new NullReferenceException("device");

			if (string.IsNullOrEmpty(device.Token))
				throw new ArgumentException("device.token");

			if (transactions == null || !transactions.Any())
				return;

			// Get transaction type, check if we have payload content for this type

			var type = transactions.First().Type;

			if (!_contents.ContainsKey(type))
				return;

			var content = _contents[type];

			// Make payload

            var notification = new AdamantApnsNotification
            {
				Payload = new ApnsPayload
				{
					Alert = new ApnsNotificationAlert(),
					Badge = 1
				},
                RecipientAddress = device.Address
            };

			if (!string.IsNullOrEmpty(content.Sound))
				notification.Payload.Sound = content.Sound;

			if (!string.IsNullOrEmpty(content.Body))
				notification.Payload.Alert.BodyLocalizationKey = content.Body;

			if (!string.IsNullOrEmpty(content.Title))
				notification.Payload.Alert.TitleLocalizationKey = content.Title;

			pusher.SendNotificationAsync(notification, device.Token);
		}


		#region Events

		void Pusher_OnNotificationSuccess(object sender, NotificationSuccessEventArgs<ApnsNotification> args)
		{
			_logger.LogDebug("Notification success, token: {0}", args.Token);
		}

		void Pusher_OnNotificationFailed(object sender, NotificationFailedEventArgs<ApnsNotification, ApnsResult> args)
		{
			_logger.LogError(args.Exception, "Notification failed. Code: {0}, reason: {1}", args.ResultCode, args.Reason);

			switch (args.ResultCode)
			{
				case ApnsResult.BadRequest:
					if (args.Reason.Equals("BadDeviceToken"))
						OnInvalidToken?.Invoke(this, new InvalidTokenEventArgs(args.Token));
					
					break;

				case ApnsResult.TokenExpired:
					OnInvalidToken?.Invoke(this, new InvalidTokenEventArgs(args.Token));
					break;
			}
		}


		#endregion


		#region Tools

		private static Dictionary<TransactionType, PayloadContent> LoadPayloadContent(IConfiguration configuration)
		{
			var contents = new Dictionary<TransactionType, PayloadContent>();

			foreach (var content in configuration.GetSection("ApplePusher").GetSection("Payload").GetChildren())
			{
				var typeRaw = content["transactionType"];

				TransactionType type;

				if (!Enum.TryParse(typeRaw, out type))
					continue;

				var title = content["title"];
				var body = content["body"];
				var sound = content["sound"];

				contents.Add(type, new PayloadContent(title, body, sound));
			}

			return contents;
		}

		#endregion
	}
}
