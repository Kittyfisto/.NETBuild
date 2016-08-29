using System;
using System.Diagnostics.Contracts;
using System.IO;

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFileName"></param>
		/// <param name="destFileName"></param>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="IOException"></exception>
		void Copy(string sourceFileName, string destFileName);
	}
}