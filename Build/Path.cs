using System;
using System.Diagnostics.Contracts;

namespace Build
{
	public static class Path
	{
		public static string Normalize(string path)
		{
			string normalized = System.IO.Path.GetFullPath(new Uri(path).LocalPath)
									.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
			return normalized;
		}

		public static string Combine(params string[] fragments)
		{
			var combined = System.IO.Path.Combine(fragments);
			return combined;
		}

		public static string GetDirectory(string fullPath)
		{
			var directory = System.IO.Path.GetDirectoryName(fullPath);
			return directory;
		}

		public static string GetDirectoryName(string path)
		{
			var name = System.IO.Path.GetDirectoryName(path);
			return name;
		}

		public static bool IsPathRooted(string path)
		{
			bool isRooted = System.IO.Path.IsPathRooted(path);
			return isRooted;
		}

		public static string GetDirectoryWithoutRoot(string fullPath, Slash finalSlash)
		{
			var directory = GetDirectory(fullPath);
			var root = System.IO.Path.GetPathRoot(directory);
			var dir = directory.Substring(root.Length);

			if (finalSlash == Slash.Include)
				dir += '\\';

			return dir;
		}

		public static string GetRelativeDir(string expandedInclude)
		{
			int i;
			for (i = expandedInclude.Length - 1; i >= 0; --i)
			{
				switch (expandedInclude[i])
				{
					case '\\':
					case '/':
						break;
				}
			}

			if (i == -1)
				return string.Empty;

			return expandedInclude.Substring(0, i);
		}

		public static string GetFilename(string fullPath)
		{
			var filename = System.IO.Path.GetFileName(fullPath);
			return filename;
		}

		public static string GetFilenameWithoutExtension(string fullPath)
		{
			var filename = System.IO.Path.GetFileNameWithoutExtension(fullPath);
			return filename;
		}

		public static string GetRootDir(string fullPath)
		{
			var root = System.IO.Path.GetPathRoot(fullPath);
			return root;
		}

		public static string GetExtension(string filePath)
		{
			int index = filePath.LastIndexOf('.');
			if (index == -1)
				return string.Empty;

			return filePath.Substring(index);
		}

		public static string MakeAbsolute(string rootDirectory, string relativeOrAbsolutePath)
		{
			if (IsPathRooted(relativeOrAbsolutePath))
				return relativeOrAbsolutePath;

			var path = System.IO.Path.Combine(rootDirectory, relativeOrAbsolutePath);
			var normalized = Normalize(path);
			return normalized;
		}

		[Pure]
		public static string MakeRelative(string relativeTo, string fullPath)
		{
			if (!IsPathRooted(fullPath))
				return fullPath;

			var directory = EnsureBackslash(relativeTo);
			var tmp = new Uri(directory);
			var relative = tmp.MakeRelativeUri(new Uri(fullPath));
			var path = relative.OriginalString.Replace('/', '\\');
			return path;
		}

		[Pure]
		public static string EnsureBackslash(string path)
		{
			if (!path.EndsWith("\\"))
				return path + "\\";

			return path;
		}
	}
}