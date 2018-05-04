using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Adamant.Models;
using Adamant.NotificationService.Models;
using Microsoft.Extensions.Configuration;
using PushSharp.Apple;

namespace Adamant.NotificationService.ApplePusher
{
	public class Pusher : IPusher
	{
		#region Properties

		public IConfiguration Configuration { get; set; }

		private ApnsServiceBroker _broker;
		private Dictionary<TransactionType, PayloadContent> _contents;

		#endregion

		#region IPusher

		public void Start()
		{
			_contents = LoadPayloadContent(Configuration);
			_broker = CreateBroker(Configuration);

			if (_broker == null)
				throw new Exception("Can't create APNs broker");

			_broker.Start();

			// TODO: Start feedback poller
		}

		public void Stop()
		{
			_broker.Stop();
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

		#region Tools

		private static ApnsServiceBroker CreateBroker(IConfiguration configuration)
		{
			if (configuration == null)
				throw new NullReferenceException("configuration");

			var certName = configuration["ApplePusher:Certificate:filename"];
			var certPass = configuration["ApplePusher:Certificate:pass"];

			if (string.IsNullOrEmpty(certName))
				throw new Exception("Can't get certerficate filename from configuration");

			var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, certName, certPass);

			var broker = new ApnsServiceBroker(config);

			broker.OnNotificationFailed += (notification, aggregateEx) =>
			{
				aggregateEx.Handle(ex =>
				{
					if (ex is ApnsNotificationException notificationException)
					{
						var apnsNotification = notificationException.Notification;
						var statusCode = notificationException.ErrorStatusCode;

						Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
					}
					else
					{
						Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
					}

					return true;
				});
			};

			broker.OnNotificationSucceeded += (notification) =>
			{
				Console.WriteLine("Apple Notification Sent!");
			};

			return broker;
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
