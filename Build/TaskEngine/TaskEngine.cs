using System;
using System.Collections.Generic;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	/// <summary>
	///     Responsible for executing the <see cref="Target" />s of a <see cref="Project" />
	///     (especially the <see cref="DomainModel.MSBuild.Node" />s within).
	/// </summary>
	public sealed class TaskEngine
	{
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;
		private readonly Project _project;
		private readonly string _target;
		private readonly Dictionary<Type, ITaskRunner> _taskRunners;
		private readonly Dictionary<string, Target> _availableTargets;

		public TaskEngine(ExpressionEngine.ExpressionEngine expressionEngine,
		                  IFileSystem fileSystem,
		                  Project project,
		                  string target,
		                  BuildEnvironment environment,
		                  ILogger logger)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (logger == null)
				throw new ArgumentNullException("logger");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
			_project = project;
			_target = target;
			_environment = environment;
			_logger = logger;

			_availableTargets = new Dictionary<string, Target>(_project.Targets.Count);
			foreach (var t in _project.Targets)
			{
				_availableTargets.Add(t.Name, t);
			}

			_taskRunners = new Dictionary<Type, ITaskRunner>
				{
					{typeof (PropertyGroup), new PropertyGroupTask(_expressionEngine, _logger, _environment)},
					{typeof (Message), new MessageTask(_expressionEngine, _logger, _environment)},
					{typeof (Warning), new WarningTask(_expressionEngine, _logger, _environment)},
					{typeof (Error), new MessageTask(_expressionEngine, _logger, _environment)},
					{typeof (Copy), new CopyTask(_expressionEngine, _fileSystem, _logger, _environment)},
					{typeof (Delete), new DeleteTask(_expressionEngine, _fileSystem, _logger, _environment)},
					{typeof (Csc), new CscTask(_expressionEngine, _fileSystem, _logger, _environment)},
					{typeof(Exec), new ExecTask(_expressionEngine, _fileSystem, _logger, _environment)}
				};
		}

		private List<Target> TryFindTargets(string expression)
		{
			var targetNames = _expressionEngine.EvaluateExpression(expression, _environment)
											   .Split(new[] { Tokenizer.ItemListSeparator }, StringSplitOptions.RemoveEmptyEntries);
			var targets = new List<Target>(targetNames.Length);
			for (int i = 0; i < targetNames.Length; ++i)
			{
				var name = targetNames[i];
				Target target;
				if (!_availableTargets.TryGetValue(name, out target))
				{
					_logger.WriteWarning("No such target \"{0}\"", name);
				}
				else
				{
					targets.Add(target);
				}
			}
			return targets;
		}

		public void Run()
		{
			var executedTargets = new HashSet<Target>();
			var pendingTargets = new TargetStack();
			var targets = TryFindTargets(_target);
			targets.Reverse();
			foreach (var pendingTarget in targets)
			{
				if (pendingTarget != null)
					pendingTargets.Push(pendingTarget);
			}

			while (pendingTargets.Count > 0)
			{
				var targetToBeExecuted = pendingTargets.Peek();
				var dependingTargets = TryFindTargets(targetToBeExecuted.DependsOnTargets);
				bool requirementsSatisfied = true;
				foreach (var target in dependingTargets)
				{
					if (!executedTargets.Contains(target))
					{
						pendingTargets.TryPush(target);
						requirementsSatisfied = false;
						break;
					}
				}

				if (requirementsSatisfied)
				{
					pendingTargets.Pop();
					Run(targetToBeExecuted);
					executedTargets.Add(targetToBeExecuted);
				}
			}
		}

		public void Run(Target target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (target.Condition != null)
			{
				if (!_expressionEngine.EvaluateCondition(target.Condition, _environment))
				{
					_logger.WriteLine(Verbosity.Diagnostic, "  Skipping target \"{0}\" because {1} does not evaluate to true",
					                  target.Name,
					                  target.Condition);
					return;
				}
			}

			target.Inputs = _expressionEngine.EvaluateConcatenation(target.Inputs, _environment);
			target.Output = _expressionEngine.EvaluateConcatenation(target.Output, _environment);
			if (IsUpToDate(target))
			{
				_logger.WriteLine(Verbosity.Normal,
				                  "  Skipping target \"{0}\" because it's inputs are up-to-date with respect to its outputs",
				                  target.Name);
			}

			_logger.WriteLine(Verbosity.Normal, "{0}:", target.Name);

			foreach (Node node in target.Children)
			{
				Run(node);
			}
		}

		private bool IsUpToDate(Target target)
		{
			// TODO:
			return false;
		}

		public void Run(Node task)
		{
			if (task == null)
				throw new ArgumentNullException("task");

			var condition = task.Condition;
			if (condition != null)
			{
				if (!_expressionEngine.EvaluateCondition(condition, _environment))
				{
					_logger.WriteLine(Verbosity.Diagnostic, "  Skipping task \"{0}\" because {1} does not evaluate to true",
					                  task.GetType().Name,
					                  condition);
					return;
				}
			}

			ITaskRunner taskRunner;
			if (!_taskRunners.TryGetValue(task.GetType(), out taskRunner))
			{
				throw new BuildException(string.Format("Unknown task: {0}", task.GetType()));
			}

			taskRunner.Run(task);
		}
	}
}