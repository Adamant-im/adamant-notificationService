using System;
using Newtonsoft.Json;
using SharpPusher;

namespace SharpPusherSample
{
    class ExtendedApnsNotification : ApnsNotification
    {
        [JsonProperty("txn-id")]
        public string TxnId { get; set; }
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

			var pusher = new ApnsPusher(keyId, teamId, bundleAppId, keyPath, keyPassword, ApnsEnvironment.Production);

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
                TxnId = ""
            };

			pusher.OnNotificationSuccess += OnNotificationSuccess;
			pusher.OnNotificationFailed += OnNotificationFailed;

			Console.WriteLine("Sending notification...");
			pusher.SendNotificationAsync(notification, deviceToken);
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
