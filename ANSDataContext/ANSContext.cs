using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adamant.NotificationService.DataContext
{
	public class ANSContext: DbContext, IDesignTimeDbContextFactory<ANSContext>
	{
		private readonly string connectionString = "";

		public DbSet<Device> Devices { get; set; }
		public DbSet<ServiceState> ServiceStates { get; set; }
		
		#region Ctor

		public ANSContext(DbContextOptions<ANSContext> options) : base(options)
		{
		}
		
		public ANSContext(string connectionString, string provider): base(OptionsWithConnectionString(connectionString, provider))
		{
		}

		public ANSContext() : base()
		{}

		public ANSContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ANSContext>();
			optionsBuilder.UseMySQL(connectionString);
			return new ANSContext(optionsBuilder.Options);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL(connectionString);
		}

		#endregion

		#region Internal

		protected static DbContextOptions<ANSContext> OptionsWithConnectionString(string connectionString, string provider)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ANSContext>();

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
