using System;

namespace Adamant
{
	public static class AdamantUtilities
	{
		public static bool IsValidAdamantAddress(string address)
		{
			if (string.IsNullOrEmpty(address))
				return false;

			return true;
		}
	}
}
