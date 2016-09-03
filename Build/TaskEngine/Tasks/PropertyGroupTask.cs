using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine.Tasks
{
	internal sealed class PropertyGroupTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public PropertyGroupTask(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");

			_expressionEngine = expressionEngine;
		}

		public void Run(BuildEnvironment environment, Node task, ILogger logger)
		{
			var group = (PropertyGroup) task;
			_expressionEngine.Evaluate(group, environment);
		}
	}
}