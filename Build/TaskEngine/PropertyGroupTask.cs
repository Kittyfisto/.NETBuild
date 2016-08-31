using System;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal sealed class PropertyGroupTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly ILogger _logger;
		private readonly BuildEnvironment _environment;

		public PropertyGroupTask(ExpressionEngine.ExpressionEngine expressionEngine, ILogger logger, BuildEnvironment environment)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (environment == null)
				throw new ArgumentNullException("environment");

			_expressionEngine = expressionEngine;
			_logger = logger;
			_environment = environment;
		}

		public void Run(Node task)
		{
			var group = (PropertyGroup) task;
			_expressionEngine.Evaluate(group, _environment);
		}
	}
}