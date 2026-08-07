using System;
using Xunit;

namespace Adamant.Tests
{
	public class UtilitiesTests
	{
		[Theory]
		[InlineData("~", "{home}")]
		[InlineData("~/.ans", "{home}/.ans")]
		[InlineData("blabla/~/~/", "blabla/~/~/")]
		[InlineData("", "")]
		[InlineData(" ", " ")]
		[InlineData(null, null)]
		[InlineData("/usr/local/", "/usr/local/")]
		public void HandleUnixHomedirectory(string path, string expected)
		{
			// Resolve {home} from $HOME, NOT from the same Environment.GetFolderPath call the code
			// under test uses. The previous version asked GetFolderPath(Personal) for both sides,
			// so it passed even when that returned "" and every "~/..." path silently collapsed to
			// "/..." — the exact bug that stopped both workers from starting on net8.
			String expectedParsed;
			if (!string.IsNullOrEmpty(expected))
				expectedParsed = expected.Replace("{home}", Environment.GetEnvironmentVariable("HOME"));
			else
				expectedParsed = expected;

			var handled = Utilities.HandleUnixHomeDirectory(path);

			Assert.Equal(expectedParsed, handled);
		}

		[Fact]
		public void HandleUnixHomeDirectory_ExpandsToAnAbsolutePathUnderHome()
		{
			// Guards the failure mode directly: "~/.ans" must never come back as "/.ans".
			var handled = Utilities.HandleUnixHomeDirectory("~/.ans");

			Assert.NotEqual("/.ans", handled);
			Assert.EndsWith("/.ans", handled);
			Assert.True(handled.Length > "/.ans".Length, $"'~' expanded to nothing: {handled}");
		}

		[Theory]
		[InlineData("U1234567890123456", true)]
		[InlineData("U123456", true)]
		[InlineData("U12345", false)]
		[InlineData("U12345678901234567890123", false)]
		[InlineData("B12345678910", false)]
		[InlineData("1U2345678910", false)]
		[InlineData("12345678910", false)]
		[InlineData("U12345d67890", false)]
		[InlineData("U12345d7890_", false)]
		[InlineData("u12345d67890", false)]
		[InlineData(" U12345d67890", false)]
		[InlineData("U12345d67890 ", false)]
		[InlineData("", false)]
		[InlineData(null, false)]
		[InlineData(" ", false)]
		public void ValidateAdamantAddress(string address, bool expected)
		{
			var valid = Utilities.IsValidAdamantAddress(address);
			Assert.Equal(expected, valid);
		}
	}
}
