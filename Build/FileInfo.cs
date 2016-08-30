using System;

namespace Build
{
	public struct FileInfo
	{
		public readonly DateTime CreatedTime;
		public readonly bool Exists;
		public readonly DateTime AccessTime;
		public readonly DateTime ModifiedTime;
		public readonly long Length;

		public FileInfo(System.IO.FileInfo info)
		{
			Exists = info.Exists;
			CreatedTime = info.CreationTime;
			AccessTime = info.LastAccessTime;
			ModifiedTime = info.LastWriteTime;
			Length = info.Length;
		}
	}
}