using System;
using Build.DomainModel;

namespace Build.Parser
{
	/// <summary>
	///     Is responsible for parsing the contents visual studio solutions and creating
	///     <see cref="Solution" /> objects, representing them.
	/// </summary>
	public sealed class SolutionParser
		: IFileParser<Solution>
	{
		public Solution Parse(string filePath)
		{
			throw new NotImplementedException();
		}
	}
}