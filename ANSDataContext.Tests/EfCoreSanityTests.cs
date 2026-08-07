using System;
using System.IO;
using System.Linq;
using Adamant.NotificationService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Adamant.NotificationService.DataContext.Tests
{
	/// <summary>
	/// EF Core is deliberately left on the 2.2 line (see ANSDataContext.csproj) since the packages
	/// still target netstandard2.0 and the app only needed its own TargetFramework raised to
	/// net8.0. These tests exist to back that decision with something more than a compile check:
	/// they exercise real migrations and CRUD against SQLite (the same code path used for MySQL,
	/// modulo the provider) on the net8.0 runtime.
	/// </summary>
	public class EfCoreSanityTests : IDisposable
	{
		private readonly string _dbPath;
		private readonly string _connectionString;

		public EfCoreSanityTests()
		{
			_dbPath = Path.Combine(Path.GetTempPath(), $"ans-test-{Guid.NewGuid():N}.db");
			_connectionString = $"Data Source={_dbPath}";
		}

		public void Dispose()
		{
			if (File.Exists(_dbPath))
				File.Delete(_dbPath);
		}

		[Fact]
		public void Migrate_CreatesSchema_AndSupportsDeviceCrud()
		{
			using (var context = new ANSContext(_connectionString, "sqlite"))
			{
				context.Database.Migrate();

				context.Devices.Add(new Device
				{
					Address = "U123",
					Token = "token-1",
					Provider = "apns",
					TransactionDate = DateTime.UtcNow,
					RegistrationDate = DateTime.UtcNow
				});
				context.SaveChanges();
			}

			using (var context = new ANSContext(_connectionString, "sqlite"))
			{
				var device = context.Devices.SingleOrDefault(d => d.Address == "U123");
				Assert.NotNull(device);
				Assert.Equal("token-1", device.Token);
			}
		}

		[Fact]
		public void Migrate_CreatesSchema_AndSupportsServiceStateRoundtrip()
		{
			using (var context = new ANSContext(_connectionString, "sqlite"))
			{
				context.Database.Migrate();

				context.ServiceStates.Add(new ServiceState { Service = "ChatPoller", LastHeight = 42, Date = DateTime.UtcNow });
				context.SaveChanges();
			}

			using (var context = new ANSContext(_connectionString, "sqlite"))
			{
				var state = context.ServiceStates.SingleOrDefault(s => s.Service == "ChatPoller");
				Assert.NotNull(state);
				Assert.Equal(42, state.LastHeight);
			}
		}
	}
}
