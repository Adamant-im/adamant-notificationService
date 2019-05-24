using Newtonsoft.Json;

namespace Adamant.NotificationService.SignalsRegistration
{
	public class DeviceInfo
	{
		[JsonProperty("token")]
		public string Token { get; set; }

		[JsonProperty("provider")]
		public string Provider { get; set; }

        /// <summary>
        /// // "add" - enable push notification, "remove" - disable notification for specific token and address
        /// </summary>
        [JsonProperty(PropertyName = "action", Required = Required.Default)]
        [JsonConverter(typeof(SignalActionConverter))]
        public SignalAction Action { get; set; } = SignalAction.add;
    }
}
