﻿using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Build.IO;
using Build.TaskEngine.Tasks;

namespace Build.TaskEngine
{
	/// <summary>
	///     Responsible for executing the <see cref="Target" />s of a <see cref="Project" />
	///     (especially the <see cref="DomainModel.MSBuild.Node" />s within).
	/// </summary>
	public sealed class TaskEngine
	{
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly IFileSystem _fileSystem;
		private readonly Dictionary<Type, ITaskRunner> _taskRunners;

		public TaskEngine(ExpressionEngine.ExpressionEngine expressionEngine,
			IFileSystem fileSystem)
		{
			if (expressionEngine == null)
				throw new ArgumentNullException("expressionEngine");
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			_expressionEngine = expressionEngine;
			_fileSystem = fileSystem;

			_taskRunners = new Dictionary<Type, ITaskRunner>
			{
				{typeof(PropertyGroup), new PropertyGroupTask(_expressionEngine)},
				{typeof(ItemGroup), new ItemGroupTask(_expressionEngine)},
				{typeof(Message), new MessageTask(_expressionEngine)},
				{typeof(Warning), new WarningTask(_expressionEngine)},
				{typeof(Error), new ErrorTask(_expressionEngine)},
				{typeof(Copy), new CopyTask(_expressionEngine, _fileSystem)},
				{typeof(Delete), new DeleteTask(_expressionEngine, _fileSystem)},
				//{typeof (Csc), new CscTask(_expressionEngine, _fileSystem)},
				{typeof(Csc), new RoslynTask(_expressionEngine, _fileSystem)},
				{typeof(Exec), new ExecTask(_expressionEngine, _fileSystem)},
				{typeof(ResolveAssemblyReference), new ResolveAssemblyReferenceTask(_expressionEngine, _fileSystem)},
				{typeof(ResolveProjectReference), new ResolveProjectReferenceTask(_expressionEngine, _fileSystem)},
				{typeof(Output), new OutputTask(_expressionEngine)}
			};
		}

		private List<Target> TryFindTargets(
			BuildEnvironment environment,
			ILogger logger,
			IReadOnlyDictionary<string, Target> availableTargets,
			string expression)
		{
			var targetNames = _expressionEngine.EvaluateExpression(expression, environment)
				.Split(new[] {Tokenizer.ItemListSeparator},
					StringSplitOptions.RemoveEmptyEntries);
			var targets = new List<Target>(targetNames.Length);
			for (var i = 0; i < targetNames.Length; ++i)
			{
				var name = targetNames[i].Trim();
				if (!string.IsNullOrEmpty(name))
				{
					Target target;
					if (!availableTargets.TryGetValue(name, out target))
						logger.WriteWarning("No such target \"{0}\"", name);
					else
						targets.Add(target);
				}
			}
			return targets;
		}

		public void Run(Project project,
			string target,
			BuildEnvironment environment,
			IProjectDependencyGraph graph,
			ILogger logger)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (graph == null)
				throw new ArgumentNullException("graph");
			if (logger == null)
				throw new ArgumentNullException("logger");

			logger.WriteLine(Verbosity.Quiet, "------ {0} started: Project: {1}, Configuration: {2} {3} ------",
				target,
				environment.Properties[Properties.MSBuildProjectName],
				environment.Properties[Properties.Configuration],
				environment.Properties[Properties.PlatformTarget]
			);

			var started = DateTime.Now;
			logger.WriteLine(Verbosity.Normal, "Build started {0}.", started);


			var availableTargets = new Dictionary<string, Target>(project.Targets.Count);
			foreach (var t in project.Targets)
				availableTargets.Add(t.Name, t);

			var executedTargets = new HashSet<Target>();
			var pendingTargets = new TargetStack();
			var targets = TryFindTargets(environment, logger, availableTargets, target);
			targets.Reverse();
			foreach (var pendingTarget in targets)
				if (pendingTarget != null)
					pendingTargets.Push(pendingTarget);

			while (pendingTargets.Count > 0)
			{
				var targetToBeExecuted = pendingTargets.Peek();
				var dependingTargets = TryFindTargets(environment, logger, availableTargets,
					targetToBeExecuted.DependsOnTargets);
				var requirementsSatisfied = true;
				foreach (var dependency in dependingTargets)
					if (!executedTargets.Contains(dependency))
					{
						pendingTargets.TryPush(dependency);
						requirementsSatisfied = false;
						break;
					}

				if (requirementsSatisfied)
				{
					pendingTargets.Pop();

					Run(environment, targetToBeExecuted, graph, logger);
					if (logger.HasErrors)
						break;

					executedTargets.Add(targetToBeExecuted);
				}
			}


			logger.WriteLine(Verbosity.Normal, "{0} succeeded.", target);
			var elapsed = DateTime.Now - started;
			logger.WriteLine(Verbosity.Minimal, "Time Elapsed {0}", elapsed);
		}

		private void Run(BuildEnvironment environment,
			Target target,
			IProjectDependencyGraph graph,
			ILogger logger)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (target.Condition != null)
				if (!_expressionEngine.EvaluateCondition(target.Condition, environment))
				{
					logger.WriteLine(Verbosity.Normal, "  Skipping target \"{0}\" because {1} does not evaluate to true",
						target.Name,
						target.Condition);
					return;
				}

			target.Inputs = _expressionEngine.EvaluateConcatenation(target.Inputs, environment);
			target.Output = _expressionEngine.EvaluateConcatenation(target.Output, environment);
			if (IsUpToDate(environment, target))
				logger.WriteLine(Verbosity.Normal,
					"  Skipping target \"{0}\" because it's inputs are up-to-date with respect to its outputs",
					target.Name);

			logger.WriteLine(Verbosity.Normal, "{0}:", target.Name);

			foreach (var node in target.Children)
			{
				Run(environment, node, graph, logger);

				if (logger.HasErrors)
					break;
			}
		}

		private bool IsUpToDate(BuildEnvironment environment, Target target)
		{
			// TODO:
			return false;
		}

		private void Run(BuildEnvironment environment,
			Node task,
			IProjectDependencyGraph graph,
			ILogger logger)
		{
			if (environment == null)
				throw new ArgumentNullException("environment");
			if (task == null)
				throw new ArgumentNullException("task");
			if (graph == null)
				throw new ArgumentNullException("graph");
			if (logger == null)
				throw new ArgumentNullException("logger");

			var condition = task.Condition;
			if (condition != null)
				if (!_expressionEngine.EvaluateCondition(condition, environment))
				{
					logger.WriteLine(Verbosity.Diagnostic, "  Skipping task \"{0}\" because {1} does not evaluate to true",
						task.GetType().Name,
						condition);
					return;
				}

			ITaskRunner taskRunner;
			if (!_taskRunners.TryGetValue(task.GetType(), out taskRunner))
				throw new BuildException(string.Format("Unknown task: {0}", task.GetType()));

			taskRunner.Run(environment, task, graph, logger);
		}
	}
}