using System;
using System.IO;
using System.Reflection;

namespace Build.Test
{
	public static class TestPath
	{
		static TestPath()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			AssemblyFolder = Path.GetDirectoryName(path);
		}

		public static readonly string AssemblyFolder;

		public static string Get(string relativePath)
		{
			var path = Path.Combine(AssemblyFolder, @"../../", relativePath);
			var normalized = Path.Normalize(path);
			return normalized;
		}
	}
}