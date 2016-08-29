using System;
using Build.DomainModel;
using Build.DomainModel.MSBuild;

namespace Build.Parser
{
	/// <summary>
	///     Is responsible for parsing the contents visual studio solutions and creating
	///     <see cref="Solution" /> objects, representing them.
	/// </summary>
	public sealed class SolutionParser
		: IFileParser<Solution>
	{
		private readonly IFileParser<Project> _csharpProjectParser;

		public SolutionParser(IFileParser<Project> csharpProjectParser)
		{
			if (csharpProjectParser == null)
				throw new ArgumentNullException("csharpProjectParser");

			_csharpProjectParser = csharpProjectParser;
		}

		public Solution Parse(string filePath)
		{
			throw new NotImplementedException();
		}
	}
}