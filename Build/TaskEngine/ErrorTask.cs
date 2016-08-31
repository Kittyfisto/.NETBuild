using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal sealed class ErrorTask
		: ITaskRunner
	{
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly ILogger _logger;

		public ErrorTask(ExpressionEngine.ExpressionEngine expressionEngine,
		                 ILogger logger,
		                 BuildEnvironment environment)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (environment == null)
				throw new ArgumentNullException("environment");

			_expressionEngine = expressionEngine;
			_environment = environment;
			_logger = logger;
		}

		public void Run(Node task)
		{
			var error = (Error)task;
			string text = _expressionEngine.EvaluateExpression(error.Text, _environment);
			_logger.WriteError("  {0}", text);
		}
	}
}