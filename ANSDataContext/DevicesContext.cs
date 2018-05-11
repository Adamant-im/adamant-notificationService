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
		
		public DevicesContext(string connectionString): base(OptionsWithConnectionString(connectionString))
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
			return OptionsWithConnectionString(connectionString);
		}

		private static DbContextOptions<DevicesContext> OptionsWithConnectionString(string connectionString)
		{
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();
			optionsBuilder.UseSqlite(connectionString);
			return optionsBuilder.Options;
		}

		#endregion
	}
}
