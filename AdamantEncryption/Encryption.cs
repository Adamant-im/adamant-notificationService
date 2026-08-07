using Sodium;
using Nacl;
using System.Text;

namespace Adamant
{
	public static class Encryption
	{
		public static string DecodeMessage(string message, string nonce, string senderPublicKey, string recipientPrivateKey)
		{
			var messageBytes = Utilities.HexToBinary(message);
			var nonceBytes = Utilities.HexToBinary(nonce);
			var publicKey = Utilities.HexToBinary(senderPublicKey);
			var secretKey = Utilities.HexToBinary(recipientPrivateKey);

			var decoded = DecodeMessage(messageBytes, nonceBytes, publicKey, secretKey);

			return Encoding.UTF8.GetString(decoded);
		}

		public static byte[] DecodeMessage(byte[] message, byte[] nonce, byte[] senderPublicKey, byte[] recipientPrivateKey)
		{
			// ADM account keys are Ed25519 (signing) keys, but NaCl's box encryption operates on
			// Curve25519 (Diffie-Hellman) keys — the conversion below is required, not optional.
			var curvePublicKey = PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(senderPublicKey);
			var curveSecretKey = PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey(recipientPrivateKey);

			return TweetNaCl.CryptoBoxOpen(message, nonce, curvePublicKey, curveSecretKey);
		}

		#region Tools

		public static byte[] HexToBytes(string hex)
		{
			return Utilities.HexToBinary(hex);
		}

		public static string BytesToHex(byte[] bytes){
			return Utilities.BinaryToHex(bytes);
		}

		#endregion
	}
}
