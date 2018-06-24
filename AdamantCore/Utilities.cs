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
	}
}
