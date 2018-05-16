using System.IO;
using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Adamant.NotificationService.DataContext
{
	public class DevicesContext: DbContext
	{
		public DbSet<Device> Devices { get; set; }
		
		#region Ctor

		public DevicesContext(DbContextOptions<DevicesContext> options) : base(options)
		{}
		
		public DevicesContext(string connectionString, string provider): base(OptionsWithConnectionString(connectionString, provider))
		{}
		
		public DevicesContext() : base(OptionsFromConfigurationFile("appsettings.json"))
		{}
		
		#endregion

		#region Internal

		private static DbContextOptions<DevicesContext> OptionsFromConfigurationFile(string json)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(json, optional: false, reloadOnChange: true);
			var configuration = builder.Build();

			var connectionString = configuration.GetConnectionString("Devices");
			var provider = configuration["Database:Provider"];
			return OptionsWithConnectionString(connectionString, provider);
		}

		private static DbContextOptions<DevicesContext> OptionsWithConnectionString(string connectionString, string provider)
		{
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();

			switch (provider.ToLower())
			{
				case null:
				case "mysql":
					optionsBuilder.UseMySQL(connectionString);
					break;

				case "sqlite":
					optionsBuilder.UseSqlite(connectionString);
					break;
			}

			return optionsBuilder.Options;
		}

		#endregion
	}
}
