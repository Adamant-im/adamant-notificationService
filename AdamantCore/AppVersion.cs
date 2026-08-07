using System;
using System.Reflection;

namespace Adamant
{
	public static class AppVersion
	{
		/// <summary>
		/// The version exactly as declared by &lt;Version&gt; in Directory.Build.props, e.g. "0.5.0".
		/// </summary>
		/// <remarks>
		/// Not <c>Assembly.GetName().Version</c>: that returns AssemblyVersion, a
		/// <see cref="System.Version"/>, which always has four components — so a declared "0.5.0"
		/// is reported as "0.5.0.0" and never matches the release tag.
		/// </remarks>
		public static string Current => Of(Assembly.GetEntryAssembly());

		public static string Of(Assembly assembly)
		{
			if (assembly == null)
				return Unknown;

			var informational = assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

			return string.IsNullOrEmpty(informational)
				? assembly.GetName().Version?.ToString() ?? Unknown
				: Normalize(informational);
		}

		public const string Unknown = "unknown";

		/// <summary>
		/// Strips the build metadata the .NET SDK appends to AssemblyInformationalVersion.
		/// </summary>
		/// <remarks>
		/// The SDK stamps it as "&lt;Version&gt;+&lt;commit sha&gt;" by default, so the raw
		/// attribute is not printable as a release version. A pre-release suffix ("-rc.1") is part
		/// of the version and is kept; only the "+build" metadata is dropped.
		/// </remarks>
		public static string Normalize(string informationalVersion)
		{
			if (string.IsNullOrEmpty(informationalVersion))
				return Unknown;

			var plus = informationalVersion.IndexOf('+');
			return plus >= 0 ? informationalVersion.Substring(0, plus) : informationalVersion;
		}
	}
}
