using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	internal sealed class WarningTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public WarningTask(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");

			_expressionEngine = expressionEngine;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			if (environment == null)
				throw new ArgumentNullException("environment");
			var warning = (Warning)task;
			var text = _expressionEngine.EvaluateExpression(warning.Text, environment);

			logger.WriteWarning("  {0}", text);
		}
	}
}