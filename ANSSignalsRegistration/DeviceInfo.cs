using Newtonsoft.Json;

namespace Adamant.NotificationService.SignalsRegistration
{
	public class DeviceInfo
	{
		[JsonProperty("token")]
		public string Token { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }

        [JsonProperty(PropertyName = "action", Required = Required.Default)]
        public string Action { get; set; } = "add"; // "add" - enable push notification, "remove" - disable notification for specific token and address 
    }
}
