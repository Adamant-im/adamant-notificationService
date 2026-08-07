using System;
using System.Text;

namespace Adamant.Api
{
	public class ServerDescription: IEquatable<ServerDescription>
	{
		public string Ip { get; }
		public string Protocol { get; }
		public int? Port { get; }

		public string Url { get; }

		private ServerDescription() {}

		public ServerDescription(string ip, string protocol, int? port)
		{
			Ip = ip;
			Protocol = protocol;
			Port = port;

			Url = BuildUrl(this);
		}

		public bool Equals(ServerDescription other)
		{
			return other != null &&
		           Ip == other.Ip &&
		           Protocol == other.Protocol &&
		           Port == other.Port;
		}

		// Implementing IEquatable<T> without these two leaves the type with two different notions
		// of equality: Where(s => !s.Equals(x)) uses value equality, while Contains, Distinct,
		// HashSet and Dictionary all fall back to reference equality. Keep them consistent.
		public override bool Equals(object obj) => Equals(obj as ServerDescription);

		public override int GetHashCode() => HashCode.Combine(Ip, Protocol, Port);

		private static string BuildUrl(ServerDescription server)
		{
			var sb = new StringBuilder();

			if (string.IsNullOrEmpty(server.Protocol))
				sb.Append("https://");
			else
				sb.AppendFormat("{0}://", server.Protocol);

			sb.Append(server.Ip);

			if (server.Port.HasValue)
				sb.AppendFormat(":{0}", server.Port.Value);

			return sb.ToString();
		}
	}
}
