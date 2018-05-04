using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace Adamant.NotificationService.DataContext
{
	public class DevicesContext: DbContext
	{
		public static DevicesContext CreateContextWithSQLite(string connectionString)
		{
			var optionsBuilder = new DbContextOptionsBuilder<DevicesContext>();
			optionsBuilder.UseSqlite(connectionString);

			return new DevicesContext(optionsBuilder.Options);
		}

		public DevicesContext(DbContextOptions<DevicesContext> options) : base(options)
		{
		}

		public DbSet<Device> Devices { get; set; }
	}
}
