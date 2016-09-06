using Build.DomainModel.MSBuild;

namespace Build.TaskEngine.Tasks
{
	public sealed class OutputTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public OutputTask(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			_expressionEngine = expressionEngine;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var output = (Output) task;

			var taskParameter = _expressionEngine.EvaluateExpression(output.TaskParameter, environment);
			var propertyName = _expressionEngine.EvaluateExpression(output.PropertyName, environment);

			string value;
			if (!environment.Output.TryGetValue(taskParameter, out value))
			{
				logger.WriteError("TaskParameter \"{0}\" not found", taskParameter);
			}
			else
			{
				environment.Properties[propertyName] = value;
			}
		}
	}
}