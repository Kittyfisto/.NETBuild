using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	public class ExecTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;
		private readonly BuildEnvironment _environment;

		public ExecTask(ExpressionEngine.ExpressionEngine expressionEngine,
		                IFileSystem fileSystem,
		                ILogger logger,
		                BuildEnvironment environment)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (environment == null)
				throw new ArgumentNullException("environment");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
			_logger = logger;
			_environment = environment;
		}

		public void Run(Node task)
		{
			var exec = (Exec) task;
			throw new NotImplementedException();
		}
	}
}