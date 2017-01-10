using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Build.IO
{
	/// <summary>
	///     Interface that encapsulates *all* access to the filesystem.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		///     The current directory.
		///     Will be used when filepaths or directory paths are not rooted.
		/// </summary>
		string CurrentDirectory { get; set; }

		[Pure]
		bool Exists(string filename);

		[Pure]
		bool ExistsDirectory(string directory);

		[Pure]
		FileInfo GetFileInfo(string filename);

		/// <summary>
		/// </summary>
		/// <param name="sourceFileName"></param>
		/// <param name="destFileName"></param>
		/// <param name="overwrite"></param>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="IOException"></exception>
		void CopyFile(string sourceFileName, string destFileName, bool overwrite);

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="IOException"></exception>
		void DeleteFile(string fileName);

		/// <summary>
		///     Creates the given directory if it doesn't exist already.
		/// </summary>
		/// <param name="directoryPath"></param>
		void CreateDirectory(string directoryPath);

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		string ReadAllText(string fileName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		byte[] ReadAllBytes(string fileName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="text"></param>
		void WriteAllText(string fileName, string text);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		void WriteAllBytes(string fileName, byte[] data);

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Stream OpenWrite(string fileName);

		/// <summary>
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Stream OpenRead(string fileName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		IEnumerable<string> EnumerateFiles(string path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		IEnumerable<string> EnumerateFiles(string path, string searchPattern);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
	}
}