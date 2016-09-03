using System;
using System.ComponentModel;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	internal sealed class MessageTask
		: ITaskRunner
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		public MessageTask(ExpressionEngine.ExpressionEngine expressionEngine)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");

			_expressionEngine = expressionEngine;
		}

		public void Run(BuildEnvironment environment, Node task, ILogger logger)
		{
			if (environment == null)
				throw new ArgumentNullException("environment");

			var message = (Message) task;
			Verbosity verbosity = ImportanceToVerbosity(message.Importance);
			var text = _expressionEngine.EvaluateExpression(message.Text, environment);

			logger.WriteLine(verbosity, "  {0}", text);
		}

		private static Verbosity ImportanceToVerbosity(Importance importance)
		{
			switch (importance)
			{
				case Importance.High:
					return Verbosity.Quiet;

				case Importance.Normal:
					return Verbosity.Normal;

				case Importance.Low:
					return Verbosity.Detailed;

				default:
					throw new InvalidEnumArgumentException("importance", (int) importance, typeof (Importance));
			}
		}
	}
}