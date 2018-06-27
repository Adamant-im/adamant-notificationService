using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Adamant.NotificationService.ApplePusher
{
	internal class Payload
	{
		#region Properties

		/// <summary>
		/// Body localisation key. Required.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Notification title localisation key. Optional.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Badge number. Optional. '0' removes badge from app icon.
		/// </summary>
		public int? Badge { get; set; }

		/// <summary>
		/// Notification sound filename on device. Optional, if not specified - plays default iOS sound.
		/// </summary>
		public string Sound { get; set; }

		#endregion

		#region ctor

		public Payload() {}

		public Payload(string title, string body, int? badge = null, string sound = null)
		{
			Title = title;
			Body = body;
			Badge = badge;
			Sound = sound;
		}

		#endregion

		#region Utilities

		public JObject ToJObject()
		{
			var root = new Dictionary<String, object>();
			var aps = new Dictionary<String, object>();
			var alert = new Dictionary<String, object>();

			root.Add("aps", aps);
			aps.Add("alert", alert);

			alert["loc-key"] = Body;

			if (!string.IsNullOrEmpty(Title))
				alert["title-loc-key"] = Title;
			
			if (Badge.HasValue)
				aps["badge"] = Badge.Value;

			if (!string.IsNullOrEmpty(Sound))
				aps["sound"] = Sound;

			return JObject.FromObject(root);
		}

		#endregion
	}
}
