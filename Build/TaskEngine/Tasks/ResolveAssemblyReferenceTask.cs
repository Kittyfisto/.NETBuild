using Build.DomainModel.MSBuild;

namespace Build.TaskEngine.Tasks
{
	public sealed class ResolveAssemblyReferenceTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public ResolveAssemblyReferenceTask(ExpressionEngine.ExpressionEngine expressionEngine, IFileSystem fileSystem)
		{
			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
		}
	}
}