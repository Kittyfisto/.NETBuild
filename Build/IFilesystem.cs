using System;
using System.Diagnostics.Contracts;
using System.IO;
using Build.DomainModel.MSBuild;

namespace Build
{
	/// <summary>
	///     Interface that encapsulates *all* access to the filesystem.
	///     Only exists to support propert testing of code that accesses the filesystem...
	/// </summary>
	public interface IFileSystem
	{
		[Pure]
		bool Exists(string filename);

		[Pure]
		FileInfo GetInfo(string filename);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="include"></param>
		/// <param name="identity"></param>
		/// <param name="environment"></param>
		[Pure]
		ProjectItem CreateProjectItem(string type, string include, string identity, BuildEnvironment environment);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFileName"></param>
		/// <param name="destFileName"></param>
		/// <param name="overwrite"></param>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="IOException"></exception>
		void CopyFile(string sourceFileName, string destFileName, bool overwrite);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absoluteFile"></param>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="IOException"></exception>
		void DeleteFile(string absoluteFile);

		/// <summary>
		/// Creates the given directory if it doesn't exist already.
		/// </summary>
		/// <param name="directoryPath"></param>
		void CreateDirectory(string directoryPath);

	}
}