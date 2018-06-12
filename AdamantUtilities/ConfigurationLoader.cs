using System.IO;
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
			var builder = new ConfigurationBuilder()
				.SetBasePath("~/.ans/")
				.AddJsonFile("config.json", false, true);

			return builder.Build();
		}
	}
}
