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
			{
				if (path.Length > 0)
					return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + path.Substring(1);

				else
					Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}

			return path;
		}

		public static DateTime ToDateTime(this double timestamp)
		{
			return MagicAdamantDate.AddSeconds(timestamp);
		}
	}
}
