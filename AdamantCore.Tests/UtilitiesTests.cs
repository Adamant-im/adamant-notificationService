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
	}
}
