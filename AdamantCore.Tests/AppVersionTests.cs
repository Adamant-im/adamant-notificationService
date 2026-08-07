using Xunit;

namespace Adamant.Tests
{
	public class AppVersionTests
	{
		[Theory]
		// The .NET SDK stamps InformationalVersion as "<Version>+<commit sha>" by default,
		// so the raw attribute is not printable as a release version.
		[InlineData("0.5.0+ac068bea0b0853b324ecb77e6b0ac60d218f94d4", "0.5.0")]
		[InlineData("0.5.0", "0.5.0")]
		// A pre-release suffix is part of the version; only "+build" metadata is noise.
		[InlineData("0.5.0-rc.1+abcdef0", "0.5.0-rc.1")]
		[InlineData("0.5.0-rc.1", "0.5.0-rc.1")]
		[InlineData("", AppVersion.Unknown)]
		[InlineData(null, AppVersion.Unknown)]
		public void Normalize_DropsOnlyTheBuildMetadata(string informationalVersion, string expected)
		{
			Assert.Equal(expected, AppVersion.Normalize(informationalVersion));
		}

		[Fact]
		public void Of_ReturnsUnknownWithoutAnAssembly()
		{
			Assert.Equal(AppVersion.Unknown, AppVersion.Of(null));
		}

		[Fact]
		public void Of_ReadsTheDeclaredVersionRatherThanTheWidenedAssemblyVersion()
		{
			// Guards the original bug: Assembly.GetName().Version widened "0.5.0" to "0.5.0.0",
			// so the logged version could never match the value in Directory.Build.props.
			var version = AppVersion.Of(typeof(AppVersionTests).Assembly);

			Assert.NotEqual(AppVersion.Unknown, version);
			Assert.DoesNotContain('+', version);
			Assert.Equal(3, version.Split('.').Length);
		}
	}
}
