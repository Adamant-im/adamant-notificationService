using Newtonsoft.Json;
using SharpPusher;

namespace Adamant.NotificationService.ApplePusher
{
    internal class AdamantApnsNotification: ApnsNotification
    {
        [JsonProperty("push-recipient")]
        public string PushRecipient { get; set; }

        [JsonProperty("txn-id")]
        public string TxnId { get; set; }
    }
}
