using Newtonsoft.Json;

namespace Adamant.Models
{
	public class TransactionAsset
	{
		[JsonProperty("chat")]
		public ChatAsset Chat { get; set; }
	}
}

/*
"asset": {
	"chat": {
	  "message": "eb331fd5492b6d7bf20867bee77a41713966aaf460593596c3c99879d752696594ca80725f3a1ce739cd686fc4edbf0a0e5698c320a8e73ff8b3f64fb3e535cb647f305a1c180cfe949502c5bed4324b27a7d79a2bb29d908da4303dd58e5755f8a846cf7e52a1799adb63c5a009539c5df0f9a47245fea515dfcd16b068ae58cadf22b7335cbf6a8a07173c5a276b94e2050a700a72bb56c83e8926ee29bfae9187395790d7990bc0ae5505df4e1f52ff276609b3267c854d",
	  "own_message": "91db73baa96edf533776fcdda017f03bb22bc5090c7255b5",
	  "type": 1
	}
}
*/