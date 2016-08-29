using System.IO;

namespace Build
{
	public sealed class FileSystem
		: IFileSystem
	{
		public bool Exists(string filename)
		{
			return File.Exists(filename);
		}

		public void Copy(string sourceFileName, string destFileName)
		{
			File.Copy(sourceFileName, destFileName);
		}
	}
}