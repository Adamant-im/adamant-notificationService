using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using SharpPusher;

namespace SharpPusherSample
{
    class ExtendedApnsNotification : ApnsNotification
    {
        [JsonProperty("txn-id")]
        public string TxnId { get; set; }

        [JsonProperty("push-recipient")]
        public string PushRecipient { get; set; }
    }

    class Program
	{
		static void Main(string[] args)
		{
			var keyId = "";
			var teamId = "";
			var bundleAppId = "";
			var keyPath = "";
			var keyPassword = "";

			var deviceToken = "";

            NLog.LogManager.LoadConfiguration("NLog.config");
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            var logger = loggerFactory.CreateLogger<ApnsPusher>();

            var pusher = new ApnsPusher(logger, keyId, teamId, bundleAppId, keyPath, keyPassword, ApnsEnvironment.Production);

			var notification = new ExtendedApnsNotification
            {
				Payload = new ApnsPayload
				{
					Alert = new ApnsNotificationAlert
					{
						TitleLocalizationKey = "NotificationsService.NewMessage.Title",
						BodyLocalizationKey = "NotificationsService.NewMessage.BodySingle"
					},
					Badge = 7,
                    MutableContent = 1
                },
                TxnId = "",
                PushRecipient = ""
            };

			pusher.OnNotificationSuccess += OnNotificationSuccess;
			pusher.OnNotificationFailed += OnNotificationFailed;

			Console.WriteLine("Sending notification...");
			pusher.SendNotificationAsync(notification, deviceToken);
            Console.ReadKey();
		}

		static void OnNotificationSuccess(object sender, NotificationSuccessEventArgs<ApnsNotification> args)
		{
			Console.WriteLine("Notification success");
		}

		static void OnNotificationFailed(object sender, NotificationFailedEventArgs<ApnsNotification, ApnsResult> args)
		{
			Console.WriteLine("Notification failed. Code: {0}, Reason: {1}", args.ResultCode, args.Reason);
		}
    }
}
