using System;
using System.Collections.Generic;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Build.Parser;
using Build.TaskEngine.Tasks;
using Node = Build.DomainModel.MSBuild.Node;

namespace Build.TaskEngine
{
	/// <summary>
	///     Responsible for executing the <see cref="Target" />s of a <see cref="Project" />
	///     (especially the <see cref="DomainModel.MSBuild.Node" />s within).
	/// </summary>
	public sealed class TaskEngine
	{
		private readonly IBuildLog _buildLog;
		private readonly Project _buildScript;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly Dictionary<Type, ITaskRunner> _taskRunners;

		public TaskEngine(ExpressionEngine.ExpressionEngine expressionEngine,
		                  IFileSystem fileSystem,
		                  IBuildLog buildLog)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");
			if (buildLog == null)
				throw new ArgumentNullException("buildLog");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;
			_buildLog = buildLog;
			_buildScript = LoadBuildScript();

			_taskRunners = new Dictionary<Type, ITaskRunner>
				{
					{typeof (PropertyGroup), new PropertyGroupTask(_expressionEngine)},
					{typeof (Message), new MessageTask(_expressionEngine)},
					{typeof (Warning), new WarningTask(_expressionEngine)},
					{typeof (Error), new ErrorTask(_expressionEngine)},
					{typeof (Copy), new CopyTask(_expressionEngine, _fileSystem)},
					{typeof (Delete), new DeleteTask(_expressionEngine, _fileSystem)},
					{typeof (Csc), new CscTask(_expressionEngine, _fileSystem)},
					{typeof (Exec), new ExecTask(_expressionEngine, _fileSystem)}
				};
		}

		private Project LoadBuildScript()
		{
			Stream stream = typeof (BuildEnvironment).Assembly.GetManifestResourceStream("Build.Microsoft.Common.props");
			Project project = ProjectParser.Instance.Parse(stream, "Common.props");
			return project;
		}

		private List<Target> TryFindTargets(
			BuildEnvironment environment,
			ILogger logger,
			IReadOnlyDictionary<string, Target> availableTargets,
			string expression)
		{
			string[] targetNames = _expressionEngine.EvaluateExpression(expression, environment)
			                                        .Split(new[] {Tokenizer.ItemListSeparator},
			                                               StringSplitOptions.RemoveEmptyEntries);
			var targets = new List<Target>(targetNames.Length);
			for (int i = 0; i < targetNames.Length; ++i)
			{
				string name = targetNames[i].Trim();
				if (!string.IsNullOrEmpty(name))
				{
					Target target;
					if (!availableTargets.TryGetValue(name, out target))
					{
						logger.WriteWarning("No such target \"{0}\"", name);
					}
					else
					{
						targets.Add(target);
					}
				}
			}
			return targets;
		}

		public void Run(Project project,
		                string target,
		                BuildEnvironment environment)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");

			// Let's inject our custom build script (we ignore the one from MSBuild)
			project = project.Merged(_buildScript);
			// Now we can start evaluating the project
			_expressionEngine.Evaluate(project, environment);

			ILogger logger = _buildLog.CreateLogger();
			var availableTargets = new Dictionary<string, Target>(project.Targets.Count);
			foreach (Target t in project.Targets)
			{
				availableTargets.Add(t.Name, t);
			}

			var executedTargets = new HashSet<Target>();
			var pendingTargets = new TargetStack();
			List<Target> targets = TryFindTargets(environment, logger, availableTargets, target);
			targets.Reverse();
			foreach (Target pendingTarget in targets)
			{
				if (pendingTarget != null)
					pendingTargets.Push(pendingTarget);
			}

			while (pendingTargets.Count > 0)
			{
				Target targetToBeExecuted = pendingTargets.Peek();
				List<Target> dependingTargets = TryFindTargets(environment, logger, availableTargets,
				                                               targetToBeExecuted.DependsOnTargets);
				bool requirementsSatisfied = true;
				foreach (Target dependency in dependingTargets)
				{
					if (!executedTargets.Contains(dependency))
					{
						pendingTargets.TryPush(dependency);
						requirementsSatisfied = false;
						break;
					}
				}

				if (requirementsSatisfied)
				{
					pendingTargets.Pop();

					Run(environment, targetToBeExecuted, logger);
					if (logger.HasErrors)
						break;

					executedTargets.Add(targetToBeExecuted);
				}
			}
		}

		public void Run(BuildEnvironment environment, Target target, ILogger logger)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (target.Condition != null)
			{
				if (!_expressionEngine.EvaluateCondition(target.Condition, environment))
				{
					logger.WriteLine(Verbosity.Diagnostic, "  Skipping target \"{0}\" because {1} does not evaluate to true",
					                 target.Name,
					                 target.Condition);
					return;
				}
			}

			target.Inputs = _expressionEngine.EvaluateConcatenation(target.Inputs, environment);
			target.Output = _expressionEngine.EvaluateConcatenation(target.Output, environment);
			if (IsUpToDate(environment, target))
			{
				logger.WriteLine(Verbosity.Normal,
				                 "  Skipping target \"{0}\" because it's inputs are up-to-date with respect to its outputs",
				                 target.Name);
			}

			logger.WriteLine(Verbosity.Normal, "{0}:", target.Name);

			foreach (Node node in target.Children)
			{
				Run(environment, node, logger);

				if (logger.HasErrors)
				{
					break;
				}
			}
		}

		private bool IsUpToDate(BuildEnvironment environment, Target target)
		{
			// TODO:
			return false;
		}

		public void Run(BuildEnvironment environment, Node task, ILogger logger)
		{
			if (task == null)
				throw new ArgumentNullException("task");

			string condition = task.Condition;
			if (condition != null)
			{
				if (!_expressionEngine.EvaluateCondition(condition, environment))
				{
					logger.WriteLine(Verbosity.Diagnostic, "  Skipping task \"{0}\" because {1} does not evaluate to true",
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

			taskRunner.Run(environment, task, logger);
		}
	}
}