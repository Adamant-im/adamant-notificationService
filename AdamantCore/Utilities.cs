using System;

namespace Adamant
{
	public static class Utilities
	{
		public static bool IsValidAdamantAddress(string address)
		{
			if (string.IsNullOrEmpty(address))
				return false;

			return true;
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
	}
}
