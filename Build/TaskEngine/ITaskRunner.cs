using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.TaskEngine
{
	internal interface ITaskRunner
	{
		void Run(BuildEnvironment environment,
		         Node task,
		         IProjectDependencyGraph graph,
		         ILogger logger);
	}
}