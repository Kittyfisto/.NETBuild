using System;
using System.Diagnostics.Contracts;
using System.IO;
using Build.DomainModel.MSBuild;

namespace Build.IO
{
	public static class FilesystemExtensions
	{
		/// <summary>
		///     Writes the contents of the given stream from its current position until its end into the file given by
		///     <paramref name="fileName" />.
		/// </summary>
		/// <remarks>
		///     This method works similar to <see cref="IFileSystem.WriteAllText" /> and <see cref="IFileSystem.WriteAllBytes" />:
		///     If no such file exists, it will be created.
		///     If the file exists, then its contents will be overwritten.
		/// </remarks>
		/// <param name="fileSystem"></param>
		/// <param name="fileName"></param>
		/// <param name="stream"></param>
		public static void WriteAllStream(this IFileSystem fileSystem, string fileName, Stream stream)
		{
			using (var file = fileSystem.OpenWrite(fileName))
			{
				stream.CopyTo(file);
			}
		}

		[Pure]
		public static ProjectItem CreateProjectItem(this IFileSystem fileSystem, string type, string include, string identity,
			BuildEnvironment environment)
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
			if (string.IsNullOrEmpty(projectDirectory))
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