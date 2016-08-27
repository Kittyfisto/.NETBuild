using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.Parser;

namespace Build.Watchdog
{
	public sealed class CSharpProjectStore
		: FileStore<CSharpProject>
	{
		public CSharpProjectStore()
			: base(CSharpProjectParser.Instance)
		{}

		public IReadOnlyDictionary<string, CSharpProject> CreateProjects()
		{
			// Currently projects are immutable and thus we don't need to clone them in any way
			var projects = new Dictionary<string, CSharpProject>(Count);
			foreach (CSharpProject project in Values)
			{
				projects.Add(project.Filename, project);
			}
			return projects;
		}
	}
}