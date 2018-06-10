using System;
using Sodium;
using Nacl;
using System.Security.Cryptography;
using System.Text;

namespace Adamant
{
	public static class Encryption
	{
		/*
		public static byte[] GenerateNonce(int length)
		{
			var provider = new RNGCryptoServiceProvider();
			var bytes = new byte[length];
			provider.GetBytes(bytes);

			return bytes;
		}

		public static (string message, string nonce) EncodeMessage(string message, string recipientPublicKey, string senderPrivateKey)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			var publicKey = Utilities.HexToBinary(recipientPublicKey);
			var secretKey = Utilities.HexToBinary(senderPrivateKey);

			var encoded = EncodeMessage(messageBytes, publicKey, secretKey);

			return (Utilities.BinaryToHex(encoded.message), Utilities.BinaryToHex(encoded.nonce));
		}

		public static (byte[] message, byte[] nonce) EncodeMessage(byte[] message, byte[] recipientPublicKey, byte[] senderPrivateKey)
		{
			var nonce = GenerateNonce(24);
			var publicKey = PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(recipientPublicKey);
			var secretKey = PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey(senderPrivateKey);

			var encodedMessage = TweetNaCl.CryptoBox(message, nonce, recipientPublicKey, senderPrivateKey);

			return (encodedMessage, nonce);
		}
		*/

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
