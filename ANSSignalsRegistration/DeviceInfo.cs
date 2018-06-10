using Newtonsoft.Json;

namespace Adamant.NotificationService.SignalsRegistration
{
	public class DeviceInfo
	{
		[JsonProperty("token")]
		public string Token { get; set; }

		[JsonProperty("os")]
		public string Os { get; set; }
	}
}
