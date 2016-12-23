using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using Build.DomainModel.MSBuild;
using log4net;

namespace Build.IO
{
	public sealed class FileSystem
		: IFileSystem
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public bool Exists(string filename)
		{
			return File.Exists(filename);
		}

		public FileInfo GetFileInfo(string filename)
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

		public void DeleteFile(string fileName)
		{
			File.Delete(fileName);
		}

		public void CreateDirectory(string directoryPath)
		{
			Directory.CreateDirectory(directoryPath);
		}

		public string ReadAllText(string fileName)
		{
			return File.ReadAllText(fileName);
		}

		public Stream OpenWrite(string fileName)
		{
			/*var path = Path.GetDirectory(fileName);
			if (!Exists(path))
				CreateDirectory(path);*/

			return File.OpenWrite(fileName);
		}

		public Stream OpenRead(string fileName)
		{
			return File.OpenRead(fileName);
		}

		public string CurrentDirectory
		{
			get { return Directory.GetCurrentDirectory(); }
			set { Directory.SetCurrentDirectory(value); }
		}

		public bool ExistsDirectory(string directory)
		{
			return Directory.Exists(directory);
		}

		public void WriteAllText(string fileName, string text)
		{
			File.WriteAllText(fileName, text);
		}

		public void WriteAllBytes(string fileName, byte[] data)
		{
			File.WriteAllBytes(fileName, data);
		}

		public byte[] ReadAllBytes(string fileName)
		{
			return File.ReadAllBytes(fileName);
		}

		public IEnumerable<string> EnumerateFiles(string path)
		{
			return Directory.EnumerateFiles(path);
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			return Directory.EnumerateFiles(path, searchPattern);
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return Directory.EnumerateFiles(path, searchPattern, searchOption);
		}
	}
}