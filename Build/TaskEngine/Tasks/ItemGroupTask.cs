using System;
using Build.DomainModel.MSBuild;

namespace Build.TaskEngine.Tasks
{
	public sealed class ItemGroupTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public ItemGroupTask(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");

			_expressionEngine = expressionEngine;
		}

		public void Run(BuildEnvironment environment, Node task, IProjectDependencyGraph graph, ILogger logger)
		{
			var group = (ItemGroup) task;
			_expressionEngine.Evaluate(group, environment);
		}
	}
}