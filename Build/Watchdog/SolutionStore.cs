using System.Collections.Generic;
using Build.DomainModel;
using Build.DomainModel.MSBuild;
using Build.Parser;

namespace Build.Watchdog
{
	public sealed class SolutionStore
		: FileStore<Solution>
	{
		public SolutionStore() : base(new SolutionParser(ProjectParser.Instance))
		{}

		public IEnumerable<Solution> CreateSolutions(IReadOnlyDictionary<string, Project> projects)
		{
			var solutions = new List<Solution>();
			foreach (Solution solution in Values)
			{
				solutions.Add(solution.Clone(projects));
			}
			return solutions;
		}
	}
}