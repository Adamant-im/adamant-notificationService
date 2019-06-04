using System;
using Newtonsoft.Json;

namespace SharpPusher
{
	public class ApnsNotification
	{
		/// <summary>
		/// Contains Apple-defined keys and is used to determine how the system receiving the notification should alert the user, if at all.
		/// </summary>
		[JsonProperty("aps")]
		public ApnsPayload Payload { get; set; }
	}

	public class ApnsPayload
	{
		/// <summary>
		/// Include this key when you want the system to display a standard alert or a banner.
		/// The notification settings for your app on the user’s device determine whether an alert or banner is displayed.
		/// </summary>
		[JsonProperty("alert")]
		public ApnsNotificationAlert Alert { get; set; }

		/// <summary>
		/// Include this key when you want the system to modify the badge of your app icon.
		/// If this key is not included in the dictionary, the badge is not changed.
		/// To remove the badge, set the value of this key to 0.
		/// </summary>
		[JsonProperty("badge")]
		public int? Badge { get; set; }

		/// <summary>
		/// Include this key when you want the system to play a sound.
		/// The value of this key is the name of a sound file in your app’s main bundle or in the Library/Sounds folder of your app’s data container. If the sound file cannot be found, or if you specify defaultfor the value, the system plays the default alert sound.
		/// </summary>
		[JsonProperty("sound")]
		public string Sound { get; set; }

		/// <summary>
		/// Include this key with a value of 1 to configure a background update notification.
		/// When this key is present, the system wakes up your app in the background and delivers the notification to its app delegate.
		/// </summary>
		[JsonProperty("content-available")]
		public int? ContentAvailable { get; set; }

		/// <summary>
		/// Provide this key with a string value that represents the notification’s type.
		/// This value corresponds to the value in the identifier property of one of your app’s registered categories.
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// Provide this key with a string value that represents the app-specific identifier for grouping notifications.
		/// If you provide a Notification Content app extension, you can use this value to group your notifications together. For local notifications, this key corresponds to the threadIdentifier property of the UNNotificationContent object.
		/// </summary>
		[JsonProperty("thread-id")]
		public string ThreadId { get; set; }

        /// <summary>
        /// Provide this key with a value of 1 to configure a mutable notification content on client side with NotificationServiceExtension.
        /// </summary>
        [JsonProperty("mutable-content")]
        public int? MutableContent { get; set; }
	}

	public class ApnsNotificationAlert
	{
		/// <summary>
		/// A short string describing the purpose of the notification.
		/// Apple Watch displays this string as part of the notification interface.
		/// This string is displayed only briefly and should be crafted so that it can be understood quickly.
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }
		/// <summary>
		/// The text of the alert message.
		/// </summary>
		[JsonProperty("body")]
		public string Body { get; set; }

		/// <summary>
		/// The key to a title string in the Localizable.strings file for the current localization.
		/// The key string can be formatted with %@ and %n$@ specifiers to take the variables specified in the title-loc-args array.
		/// </summary>
		[JsonProperty("title-loc-key")]
		public string TitleLocalizationKey { get; set; }
		/// <summary>
		/// Variable string values to appear in place of the format specifiers in title-loc-key.
		/// </summary>
		[JsonProperty("title-loc-args")]
		public string[] TitleLocalizationArgs { get; set; }

		/// <summary>
		/// A key to an alert-message string in a Localizable.strings file for the current localization (which is set by the user’s language preference).
		/// The key string can be formatted with %@ and %n$@ specifiers to take the variables specified in the loc-args array. 
		/// </summary>
		[JsonProperty("loc-key")]
		public string BodyLocalizationKey { get; set; }
		/// <summary>
		/// Variable string values to appear in place of the format specifiers in loc-key.
		/// </summary>
		[JsonProperty("loc-args")]
		public string[] BodyLocalizationArgs { get; set; }

		/// <summary>
		/// If a string is specified, the system displays an alert that includes the Close and View buttons.
		/// The string is used as a key to get a localized string in the current localization to use for the right button’s title instead of “View”.
		/// </summary>
		[JsonProperty("action-loc-key")]
		public string ActionLocalizationKey { get; set; }

		/// <summary>
		/// The filename of an image file in the app bundle, with or without the filename extension.
		/// The image is used as the launch image when users tap the action button or move the action slider.
		/// If this property is not specified, the system either uses the previous snapshot, uses the image identified by the UILaunchImageFile key in the app’s Info.plist file, or falls back to Default.png.
		/// </summary>
		[JsonProperty("launch-image")]
		public string LaunchImage { get; set; }
	}
}
