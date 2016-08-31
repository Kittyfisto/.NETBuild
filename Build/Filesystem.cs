using System;
using System.IO;
using System.Reflection;
using System.Security;
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

		public void CopyFile(string sourceFileName, string destFileName)
		{
			File.Copy(sourceFileName, destFileName);
		}

		public void DeleteFile(string absoluteFile)
		{
			File.Delete(absoluteFile);
		}
	}
}