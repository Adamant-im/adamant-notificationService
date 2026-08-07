using System;
using Microsoft.Extensions.Configuration;

namespace Adamant
{
	public static class ConfigurationLoader
	{
		/// <summary>
		/// Loads Configuration from '~/.ans'
		/// </summary>
		/// <returns>The default configuration.</returns>
		public static IConfigurationRoot GetConfiguration()
		{
			// SpecialFolder.Personal resolves through XDG on Unix and comes back empty on a
			// headless server, which made this base path "/.ans/" and crashed both workers on
			// startup. Reuse the shared "~" expansion so there is exactly one definition of home.
			var builder = new ConfigurationBuilder()
				.SetBasePath(Utilities.HandleUnixHomeDirectory("~/.ans/"))
				.AddJsonFile("config.json", false, true);

			return builder.Build();
		}
	}
}
