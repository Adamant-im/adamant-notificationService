using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

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
		
		#endregion

		#region Internal

		private static DbContextOptions<DevicesContext> OptionsWithConnectionString(string connectionString, string provider)
		{
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();

			switch (provider?.ToLower())
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
