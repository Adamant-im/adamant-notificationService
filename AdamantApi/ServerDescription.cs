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

		public ServerDescription(string ip, string protocol, string port)
		{
			Ip = ip;
			Protocol = protocol;

			if (int.TryParse(port, out int p))
				Port = p;
			else
				Port = null;

			Url = BuildUrl(this);
		}

		public bool Equals(ServerDescription other)
		{
			return Ip == other.Ip &&
		           Protocol == other.Protocol &&
		           Port == other.Port;
		}

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
