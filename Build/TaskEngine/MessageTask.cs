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
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly ILogger _logger;

		public MessageTask(ExpressionEngine.ExpressionEngine expressionEngine, ILogger logger, BuildEnvironment environment)
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
			var message = (Message) task;
			Verbosity verbosity = ImportanceToVerbosity(message.Importance);
			var text = _expressionEngine.EvaluateExpression(message.Text, _environment);

			_logger.WriteLine(verbosity, "  {0}", text);
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