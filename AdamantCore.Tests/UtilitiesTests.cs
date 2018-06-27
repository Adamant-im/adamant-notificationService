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
			String expectedParsed;
			if (!string.IsNullOrEmpty(expected))
				expectedParsed = expected.Replace("{home}", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			else
				expectedParsed = expected;
			
			var handled = Utilities.HandleUnixHomeDirectory(path);

			Assert.Equal(expectedParsed, handled);
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
