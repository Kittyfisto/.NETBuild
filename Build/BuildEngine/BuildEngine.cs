﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Build.DomainModel;
using Build.DomainModel.MSBuild;
using Build.IO;
using Build.Parser;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for building a list of (Visual Studio) solutions and/or projects.
	/// </summary>
	public sealed class BuildEngine
		: IDisposable
	{
		private readonly Arguments _arguments;
		private readonly Project _buildScript;
		private readonly IFileParser<Project> _csharpProjectParser;
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;

		private readonly IFileParser<Solution> _solutionParser;
		private readonly TaskEngine.TaskEngine _taskEngine;
		private readonly IFileSystem _fileSystem;

		public BuildEngine(Arguments arguments)
			: this(arguments, new FileSystem())
		{
		}

		public BuildEngine(Arguments arguments, IFileSystem fileSystem)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			if (arguments.MaxCpuCount <= 0 || arguments.MaxCpuCount > 1024)
				throw new BuildException(
					string.Format(
						"error MSB1032: Maximum CPU count is not valid. Value must be an integer greater than zero and nore more than 1024.\r\nSwitch: {0}",
						arguments.MaxCpuCount));

			_fileSystem = fileSystem;
			var buildlogStream = _fileSystem.OpenWrite("buildlog.txt");
			Log = new BuildLog(arguments, buildlogStream, true);
			_csharpProjectParser = new ProjectParser(_fileSystem);
			_solutionParser = new SolutionParser(_csharpProjectParser);
			_expressionEngine = new ExpressionEngine.ExpressionEngine(fileSystem);
			_taskEngine = new TaskEngine.TaskEngine(_expressionEngine, fileSystem);
			_arguments = arguments;
			if (_arguments.Targets.Count == 0)
				_arguments.Targets.Add(Targets.Build);
			else
				_arguments.Targets.Sort(new TargetComparer());

			_environment = new BuildEnvironment(name: "Build Engine Environment");
			foreach (var property in arguments.Properties)
				_environment.Properties.Add(property.Name, property.Value);

			_buildScript = LoadBuildScript();
		}

		public BuildLog Log { get; }

		public void Dispose()
		{
			Log.Dispose();
		}

		private Project LoadBuildScript()
		{
			var stream = typeof(BuildEnvironment).Assembly.GetManifestResourceStream("Build.Microsoft.Common.props");
			var project = new ProjectParser(_fileSystem).Parse(stream, "Common.props");
			return project;
		}

		private void PrintLogo()
		{
			if (_arguments.NoLogo)
				return;

			Log.FormatLine(Verbosity.Quiet, "Kittyfisto's .NET Build Engine version {0}",
				Assembly.GetCallingAssembly().GetName().Version);
			Log.FormatLine(Verbosity.Quiet, "[Microsoft .NET Framework, version {0}]", Environment.Version);
		}

		/// <summary>
		/// </summary>
		public void Execute()
		{
			PrintLogo();

			if (_arguments.Help)
				PrintHelp();
			else
				Build();
		}

		private void Build()
		{
			// Building is done in a few simple steps:

			// #1: Parse all relevant .csproj files into memory
			var projects = LoadInputFile();

			var targets = _arguments.Targets.ToList();
			if (targets.Count > 1)
				throw new BuildException("Building more than one target is not supported!");

			Build(projects, targets[0]);
		}

		private void Build(List<Project> projects, string target)
		{
			// #2: Evaluate these projects using the given environment
			// TODO: What do we do when we have conditions that require the presence of files that are from a previous step's output?
			var dependencyGraph = Evaluate(projects, _environment);

			var nodes = new BuildNode[_arguments.MaxCpuCount];
			for (var i = 0; i < nodes.Length; ++i)
			{
				var name = string.Format("Builder #{0}", i);
				nodes[i] = new BuildNode(dependencyGraph,
					_taskEngine,
					Log,
					name,
					target);
			}

			dependencyGraph.FinishedEvent.Wait();

			foreach (var builder in nodes)
				builder.Stop();

			Log.FormatLine(Verbosity.Quiet, "========== Build: {0} succeeded, {1} failed, {2} up-to-date, {3} skipped ==========",
				dependencyGraph.SucceededCount,
				dependencyGraph.FailedCount,
				"TODO",
				"TODO");
		}

		private void PrintHelp()
		{
			Log.WriteLine("TODO");
		}

		private List<Project> LoadInputFile()
		{
			var projects = new List<Project>();
			var inputFile = _arguments.InputFile;
			if (inputFile == null)
				inputFile = FindInputFile();
			else if (!Path.IsPathRooted(inputFile))
				inputFile = Path.Normalize(Path.Combine(Directory.GetCurrentDirectory(), inputFile));

			var extension = Path.GetExtension(inputFile);
			switch (extension)
			{
				case ".sln":
					var solution = _solutionParser.Parse(inputFile);
					projects.AddRange(solution.Projects);
					break;

				case ".csproj":
					var project = _csharpProjectParser.Parse(inputFile);
					projects.Add(project);
					break;

				default:
					throw new BuildException(string.Format("error MSB1009: Project file does not exist.\r\nSwitch: {0}", inputFile));
			}

			return projects;
		}

		private string FindInputFile()
		{
			var directory = _fileSystem.CurrentDirectory;
			var files = _fileSystem.EnumerateFiles(directory, "*.sln", SearchOption.TopDirectoryOnly)
				.Concat(_fileSystem.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly))
				.ToList();
			if (files.Count == 0)
				throw new BuildException(
					"error MSB1003: Specify a project or solution file. The current working directory does not contain a project or solution file.");

			return files[0];
		}

		private ProjectDependencyGraph Evaluate(List<Project> rootProjects, BuildEnvironment environment)
		{
			var projectStack = new Stack<Tmp>(rootProjects.Count);
			foreach (var project in rootProjects)
				projectStack.Push(new Tmp {Project = project});

			var graph = new ProjectDependencyGraph();

			while (projectStack.Count > 0)
			{
				var pair = projectStack.Pop();
				var rootProject = pair.Project;

				// Evaluating a project means determining the values of properties, the
				// presence of nodes, etc...
				// Properties of a project (for example $(Configuration)) are written
				// back to the environment, but we don't want to one project's enviroment
				// to interfere with the next one, thus we create one environment for each
				// project.
				var projectEnvironment = new BuildEnvironment(environment);
				var evaluated = _expressionEngine.Evaluate(rootProject.Merged(_buildScript), projectEnvironment);
				graph.Add(evaluated, projectEnvironment);

				if (pair.NeededBy != null)
					graph.AddDependency(pair.NeededBy, evaluated);

				// Now that we've evaluated the project it's time to find out if it references
				// any other projects. This is done afterwards because dependencies might be conditional
				// and thus we won't know the dependencies until after evaluating them against the
				// current build environment.
				// Anyways, we'll simply add any referenced project to the list of projects so that
				// we can build all the things!
				var references = projectEnvironment.Items.Where(x => x.Type == Items.ProjectReference);
				foreach (var reference in references)
				{
					var path = reference.Include;
					if (!Path.IsPathRooted(path))
						path = Path.Combine(projectEnvironment.Properties[Properties.MSBuildProjectDirectory], path);
					path = Path.Normalize(path);

					if (!graph.Contains(path))
					{
						var project = _csharpProjectParser.Parse(path);
						projectStack.Push(new Tmp
						{
							Project = project,
							NeededBy = evaluated
						});
					}
				}
			}

			return graph;
		}

		private struct Tmp
		{
			public Project Project;
			public Project NeededBy;
		}
	}
}