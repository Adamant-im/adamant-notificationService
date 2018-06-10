using Xunit;

namespace Adamant.Tests
{
	public class EncryptionTests
	{
		//[Theory]
		//[InlineData(
		//	/* Message */ "common",
		//	/* senderPublicKey */ "8007a01493bb4b21ec67265769898eb19514d9427bd7b701f96bc9880a6e209f",
		//	/* senderPrivateKey */ "9001490b166816af75a15a3e2b0174bfe3be3dfaa63147b4f780ed3ab90ffeab8007a01493bb4b21ec67265769898eb19514d9427bd7b701f96bc9880a6e209f",
		//	/* recipientPublicKey */ "9f895a201fd92cc60ef02d2117d53f00dc2981903cb64b2f214777269b882209",
		//	/* recipientPrivateKey */ "e91ee8e6a23ac5ff9452a15a3fbd14098dc2c6a5abf6b12464b09eb033580b6d9f895a201fd92cc60ef02d2117d53f00dc2981903cb64b2f214777269b882209"
		//)]
		//public void Encode(string raw, string senderPublicKey, string senderPrivateKey, string recipientPublicKey, string recipientPrivateKey)
		//{
		//	var encoded = Encryption.EncodeString(raw, senderPublicKey, recipientPrivateKey);

		//	Assert.NotNull(encoded);
		//	Assert.NotNull(encoded.message);
		//	Assert.NotNull(encoded.nonce);

		//	//var decoded = Encryption.DecodeMessage(encoded.message, encoded.nonce, senderPublicKey, recipientPrivateKey);

		//	//Assert.Equal(raw, decoded);
		//}

		[Theory]
		[InlineData(
			"09af1ce7e5ed484ddca3c6d1410cbf4f793ea19210e7", // encoded message
			"31caaee2d35dcbd8b614e9d6bf6095393cb5baed259e7e37", // nonce
			"9f895a201fd92cc60ef02d2117d53f00dc2981903cb64b2f214777269b882209", // sender public key
			"9001490b166816af75a15a3e2b0174bfe3be3dfaa63147b4f780ed3ab90ffeab8007a01493bb4b21ec67265769898eb19514d9427bd7b701f96bc9880a6e209f", // recipient private
			"common" // decoded expected
		)]
		public void Decode(string encoded, string nonce, string senderPublicKey, string recipientPrivateKey, string decodedExpected)
		{
			var decoded = Encryption.DecodeMessage(encoded, nonce, senderPublicKey, recipientPrivateKey);

			Assert.Equal(decodedExpected, decoded);
		}
	}
}
