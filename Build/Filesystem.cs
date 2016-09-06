using System;
using System.IO;
using System.Reflection;
using System.Security;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build
{
	public sealed class FileSystem
		: IFileSystem
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public bool Exists(string filename)
		{
			return File.Exists(filename);
		}

		public FileInfo GetInfo(string filename)
		{
			if (!Exists(filename))
				return new FileInfo();

			try
			{
				var info = new System.IO.FileInfo(filename);
				return new FileInfo(info);
			}
			catch (SecurityException e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new FileInfo();
			}
			catch (UnauthorizedAccessException e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new FileInfo();
			}
			catch (IOException e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new FileInfo();
			}
		}

		public ProjectItem CreateProjectItem(string type, string include, string identity, BuildEnvironment environment)
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

			string projectDirectory = environment.Properties[Properties.MSBuildProjectDirectory];
			if (string.IsNullOrEmpty(projectDirectory))
				throw new ArgumentException(string.Format("Expected \"$(MSBuildProjectDirectory)\" to contain a valid path, but it is empty!"));

			string fullPath = Path.IsPathRooted(include)
				                  ? include
				                  : Path.Normalize(Path.Combine(projectDirectory, include));

			var info = GetInfo(fullPath);

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

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
		{
			if (sourceFileName == null)
				throw new ArgumentNullException("sourceFileName");
			if (destFileName == null)
				throw new ArgumentNullException("destFileName");
			if (!Path.IsPathRooted(sourceFileName))
				throw new ArgumentException("sourceFileName must be rooted");
			if (!Path.IsPathRooted(destFileName))
				throw new ArgumentException("destFileName must be rooted");

			var destinationPath = Path.GetDirectory(destFileName);
			CreateDirectory(destinationPath);

			File.Copy(sourceFileName, destFileName, overwrite);
		}

		public void DeleteFile(string absoluteFile)
		{
			File.Delete(absoluteFile);
		}

		public void CreateDirectory(string directoryPath)
		{
		//	if (!Directory.Exists(outputPath))
			Directory.CreateDirectory(directoryPath);
		}
	}
}