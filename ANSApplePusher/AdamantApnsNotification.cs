using Newtonsoft.Json;
using SharpPusher;

namespace Adamant.NotificationService.ApplePusher
{
    class AdamantApnsNotification: ApnsNotification
    {
        [JsonProperty("recipient-address")]
        public string RecipientAddress { get; set; }
    }
}
