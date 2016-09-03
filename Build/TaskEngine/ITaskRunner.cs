using Build.BuildEngine;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal interface ITaskRunner
	{
		void Run(BuildEnvironment environment, Node task, ILogger logger);
	}
}