using System;
using System.Text.RegularExpressions;

namespace Adamant
{
	public static class Utilities
	{
		static DateTime MagicAdamantDate = new DateTime(2017, 9, 2, 17, 0, 0, DateTimeKind.Utc);
		static String AddressRegex = "^U([0-9]{6,20})$";

		public static bool IsValidAdamantAddress(string address)
		{
			if (string.IsNullOrEmpty(address))
				return false;

			return Regex.IsMatch(address, AddressRegex);
		}

		/// <summary>
		/// Convert unix '~' symbol to user homedirectory
		/// </summary>
		/// <returns>The unix home directory.</returns>
		/// <param name="path">Path.</param>
		public static string HandleUnixHomeDirectory(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			if (path[0] == '~')
				return GetHomeDirectory() + path.Substring(1);

			return path;
		}

		/// <summary>
		/// The current user's home directory.
		/// </summary>
		/// <remarks>
		/// Must be <c>UserProfile</c>, not <c>Personal</c>. On Unix .NET resolves
		/// <c>SpecialFolder.Personal</c> through XDG and returns an <em>empty string</em> when
		/// XDG_DOCUMENTS_DIR is not configured — which is the normal case on a headless server.
		/// That silently turned every "~/.ans/..." path into "/.ans/...", so the service could not
		/// find its config, its APNs key or its NLog config. <c>UserProfile</c> is documented as
		/// $HOME on Unix and is what this code always meant.
		/// </remarks>
		private static string GetHomeDirectory()
		{
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			// GetFolderPath returns "" rather than throwing when it cannot resolve the folder.
			// Fall back to $HOME directly so a misconfigured environment fails loudly on a real
			// path instead of quietly rooting everything at "/".
			if (string.IsNullOrEmpty(home))
				home = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;

			return home;
		}

		public static DateTime ToDateTime(this double timestamp)
		{
			return MagicAdamantDate.AddSeconds(timestamp);
		}
	}
}
