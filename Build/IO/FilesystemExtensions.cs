using System;
using System.Diagnostics.Contracts;
using Build.DomainModel.MSBuild;

namespace Build.IO
{
	public static class FilesystemExtensions
	{
		[Pure]
		public static ProjectItem CreateProjectItem(this IFileSystem fileSystem, string type, string include, string identity, BuildEnvironment environment)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (include == null)
				throw new ArgumentNullException("include");

			var item = new ProjectItem
			{
				Type = type,
				Include = include
			};

			var projectDirectory = environment.Properties[Properties.MSBuildProjectDirectory];
			if (String.IsNullOrEmpty(projectDirectory))
				throw new ArgumentException("Expected \"$(MSBuildProjectDirectory)\" to contain a valid path, but it is empty!");

			var fullPath = Path.IsPathRooted(include)
				? include
				: Path.Normalize(Path.Combine(projectDirectory, include));

			var info = fileSystem.GetFileInfo(fullPath);

			item[Metadatas.FullPath] = fullPath;
			item[Metadatas.RootDir] = Path.GetRootDir(fullPath);
			item[Metadatas.Filename] = Path.GetFilenameWithoutExtension(fullPath);
			item[Metadatas.Extension] = Path.GetExtension(fullPath);
			item[Metadatas.RelativeDir] = Path.GetRelativeDir(include);
			item[Metadatas.Directory] = Path.GetDirectoryWithoutRoot(fullPath, Slash.Include);
			item[Metadatas.Identity] = identity;
			item[Metadatas.ModifiedTime] = FormatTime(info.ModifiedTime);
			item[Metadatas.CreatedTime] = FormatTime(info.CreatedTime);
			item[Metadatas.AccessedTime] = FormatTime(info.AccessTime);

			return item;
		}

		private static string FormatTime(DateTime lastWriteTime)
		{
			var value = lastWriteTime.ToString("yyyy-mm-dd hh:mm:ss.fffffff");
			return value;
		}
	}
}