using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	public class ExecTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;

		public ExecTask(ExpressionEngine.ExpressionEngine expressionEngine,
		                IFileSystem fileSystem)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
		}

		public void Run(BuildEnvironment environment, Node task, ILogger logger)
		{
			var exec = (Exec) task;
		}
	}
}