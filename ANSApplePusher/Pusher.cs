using System;
using System.Collections.Generic;
using System.Linq;
using Adamant.Models;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PushSharp.Apple;

namespace Adamant.NotificationService.ApplePusher
{
	public class Pusher : IPusher
	{
		#region Properties

		private readonly ILogger<Pusher> _logger;
		private readonly IConfiguration _configuration;

		private ApnsServiceBroker _broker;
		private Dictionary<TransactionType, PayloadContent> _contents;

		#endregion

		#region Ctor

		public Pusher(ILogger<Pusher> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		#endregion

		#region IPusher

		public void Start()
		{
			_contents = LoadPayloadContent(_configuration);
			_broker = CreateBroker(_configuration);

			if (_broker == null)
				throw new Exception("Can't create APNs broker");

			_broker.OnNotificationFailed += OnNotificationFailed;
			_broker.OnNotificationSucceeded += OnNotificationSucceeded;

			_broker.Start();

			// TODO: Start feedback poller
		}

		public void Stop()
		{
			_broker.Stop();

			_broker.OnNotificationFailed -= OnNotificationFailed;
			_broker.OnNotificationSucceeded -= OnNotificationSucceeded;
			_broker = null;
			_contents = null;
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


			// Make payload and notification

			var payload = new Payload
			{
				Body = content.Body,
				Sound = content.Sound
			};

			var notification = new ApnsNotification
			{
				DeviceToken = device.Token,
				Payload = payload.ToJObject()
			};


			// Send it

			_broker.QueueNotification(notification);
		}

		#endregion

		#region Logging

		private void OnNotificationSucceeded(ApnsNotification notification)
		{
			_logger.LogDebug("Apple Notification Sent. Device: {0}. Payload: {1}", notification.DeviceToken, notification.Payload);
		}

		private void OnNotificationFailed(ApnsNotification notification, AggregateException aggregateEx)
		{
			aggregateEx.Handle(ex =>
			{
				if (ex is ApnsNotificationException notificationException)
				{
					var apnsNotification = notificationException.Notification;
					var statusCode = notificationException.ErrorStatusCode;

					_logger.LogError(notificationException, "Apple Notification Failed: ID={0}, Code={1}, Token={2}, Payload={3}", apnsNotification.Identifier, statusCode, notification.DeviceToken, notification.Payload);
				}
				else
				{
					_logger.LogError(ex.InnerException, "Apple Notification Failed for some unknown reason, Token={0}, Payload={1}", notification.DeviceToken, notification.Payload);
					Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
				}

				return true;
			});
		}

		#endregion

		#region Tools

		private static ApnsServiceBroker CreateBroker(IConfiguration configuration)
		{
			if (configuration == null)
				throw new NullReferenceException("configuration");

			var certName = configuration["ApplePusher:Certificate:path"];
			var certPass = configuration["ApplePusher:Certificate:pass"] ?? "";

			if (string.IsNullOrEmpty(certName))
				throw new Exception("Can't get certerficate filename from configuration");

			if (certName.Contains('~')) {
				certName = certName.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			}

			var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, certName, certPass);

			return new ApnsServiceBroker(config);
		}

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
