using System;
using Newtonsoft.Json;
using SharpPusher;
using Xunit;

namespace SharpPusherTests
{
	public class UnitTest1
	{
		[Fact]
		public void ApnsSerialization()
		{
			var notification = new ApnsNotification
			{
				Payload = new ApnsPayload
				{
					Alert = new ApnsNotificationAlert
					{
						BodyLocalizationKey = "GAME_PLAY_REQUEST_FORMAT",
						BodyLocalizationArgs = new string[] { "Jenna", "Frank" }
					},
					Sound = "chime.aiff"
				}
			};
			var expected = "{\"aps\":{\"alert\":{\"loc-key\":\"GAME_PLAY_REQUEST_FORMAT\",\"loc-args\":[\"Jenna\",\"Frank\"]},\"sound\":\"chime.aiff\"}}";
			
			var serialized = JsonConvert.SerializeObject(notification, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			Assert.Equal(expected, serialized);
		}

        [Fact]
        public void ApnsSerializationMutableContent()
        {
            var notification = new ApnsNotification
            {
                Payload = new ApnsPayload
                {
                    Alert = new ApnsNotificationAlert
                    {
                        BodyLocalizationKey = "GAME_PLAY_REQUEST_FORMAT",
                        BodyLocalizationArgs = new string[] { "Jenna", "Frank" }
                    },
                    Sound = "chime.aiff",
                    MutableContent = 1
                }
            };
            var expected = "{\"aps\":{\"alert\":{\"loc-key\":\"GAME_PLAY_REQUEST_FORMAT\",\"loc-args\":[\"Jenna\",\"Frank\"]},\"sound\":\"chime.aiff\",\"mutable-content\":1}}";

            var serialized = JsonConvert.SerializeObject(notification, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Assert.Equal(expected, serialized);
        }
    }
}
