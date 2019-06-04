using Newtonsoft.Json;

namespace SharpPusher
{
	internal class ApnsResponse
	{
		[JsonProperty("reason")]
		public string Reason { get; set; }
	}
}
