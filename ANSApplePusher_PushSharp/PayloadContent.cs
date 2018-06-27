using System;

namespace Adamant.NotificationService.ApplePusher
{
	internal struct PayloadContent
	{
		public string Title { get; }
		public string Body { get; }
		public string Sound { get; }

		public PayloadContent(string title, string body, string sound)
		{
			Title = title;
			Body = body;
			Sound = sound;
		}
	}
}
