using System.Diagnostics.Contracts;
using Build.DomainModel;

namespace Build.Parser
{
	public interface IFileParser<out T>
		where T : class, IFile
	{
		/// <summary>
		///     Reads the given file into memory and parses it into an entity.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Pure]
		T Parse(string filePath);
	}
}