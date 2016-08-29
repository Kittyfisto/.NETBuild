using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Build.DomainModel;
using Build.DomainModel.MSBuild;
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
		private readonly IFileParser<Project> _csharpProjectParser;
		private readonly BuildEnvironment _environment;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly BuildLog _log;
		private readonly AssemblyResolver _resolver;
		private readonly IFileParser<Solution> _solutionParser;
		private readonly FileSystem _fileSystem;

		public BuildEngine(Arguments arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			if (arguments.MaxCpuCount <= 0 || arguments.MaxCpuCount > 1024)
				throw new BuildException(
					string.Format(
						"error MSB1032: Maximum CPU count is not valid. Value must be an integer greater than zero and nore more than 1024.\r\nSwitch: {0}",
						arguments.MaxCpuCount));

			_log = new BuildLog(arguments);
			_fileSystem = new FileSystem();
			_expressionEngine = new ExpressionEngine.ExpressionEngine(_fileSystem);
			_csharpProjectParser = CSharpProjectParser.Instance;
			_solutionParser = new SolutionParser(_csharpProjectParser);
			_resolver = new AssemblyResolver(_expressionEngine);
			_arguments = arguments;
			if (_arguments.Targets.Count == 0)
			{
				_arguments.Targets.Add(Targets.Build);
			}
			else
			{
				_arguments.Targets.Sort(new TargetComparer());
			}

			_environment = new BuildEnvironment(name: "Build Engine Environment");
			foreach (Property property in arguments.Properties)
			{
				_environment.Add(property.Name, property.Value);
			}
		}

		public void Dispose()
		{
			_log.Dispose();
		}

		private void PrintLogo()
		{
			if (_arguments.NoLogo)
				return;

			_log.WriteLine(Verbosity.Quiet, "Kittyfisto's .NET Build Engine version {0}", Assembly.GetCallingAssembly().GetName().Version);
			_log.WriteLine(Verbosity.Quiet, "[Microsoft .NET Framework, version {0}]", Environment.Version);
		}

		/// <summary>
		/// </summary>
		public void Execute()
		{
			PrintLogo();

			if (_arguments.Help)
			{
				PrintHelp();
			}
			else
			{
				Build();
			}
		}

		private void Build()
		{
			// Building is done in a few simple steps:

			// #1: Parse all relevant .csproj files into memory
			List<Project> projects = Parse();

			var targets = _arguments.Targets.ToList();
			targets.Sort(new TargetComparer());

			foreach (var target in targets)
			{
				Build(projects, target);
			}
		}

		private void Build(List<Project> projects, string target)
		{
			// #2: Evaluate these projects using the given environment
			// TODO: What do we do when we have conditions that require the presence of files that are from a previous step's output?
			Dictionary<Project, BuildEnvironment> evaluatedProjects = Evaluate(projects, _environment);

			var dependencyGraph = new ProjectDependencyGraph(evaluatedProjects);
			var nodes = new Node[_arguments.MaxCpuCount];
			for (int i = 0; i < nodes.Length; ++i)
			{
				string name = string.Format("Builder #{0}", i);
				nodes[i] = new Node(dependencyGraph,
				                    _resolver,
				                    _log,
				                    name,
				                    target);
			}

			dependencyGraph.FinishedEvent.Wait();

			foreach (var builder in nodes)
			{
				builder.Stop();
			}

			_log.WriteLine(Verbosity.Quiet, "========== Build: {0} succeeded, {1} failed, {2} up-to-date, {3} skipped ==========",
			               dependencyGraph.SucceededCount,
			               dependencyGraph.FailedCount,
			               "TODO",
			               "TODO");
		}

		private void PrintHelp()
		{
			_log.WriteLine("TODO");
		}

		private List<Project> Parse()
		{
			var projects = new List<Project>();
			string inputFile = _arguments.InputFile;
			if (inputFile == null)
				inputFile = FindInputFile();

			string extension = Path.GetExtension(inputFile);
			switch (extension)
			{
				case ".sln":
					Solution solution = _solutionParser.Parse(inputFile);
					projects.AddRange(solution.Projects);
					break;

				case ".csproj":
					Project project = _csharpProjectParser.Parse(inputFile);
					projects.Add(project);
					break;

				default:
					throw new BuildException(string.Format("error MSB1009: Project file does not exist.\r\nSwitch: {0}", inputFile));
			}

			return projects;
		}

		private string FindInputFile()
		{
			string directory = Directory.GetCurrentDirectory();
			var files = Directory.EnumerateFiles(directory, "*.sln", SearchOption.TopDirectoryOnly)
			                     .Concat(Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly))
			                     .ToList();
			if (files.Count == 0)
			{
				throw new BuildException(
					"error MSB1003: Specify a project or solution file. The current working directory does not contain a project or solution file.");
			}

			return files[0];
		}

		private Dictionary<Project, BuildEnvironment> Evaluate(List<Project> projects, BuildEnvironment environment)
		{
			var evaluatedProjects = new Dictionary<Project, BuildEnvironment>(projects.Count);
			foreach (Project project in projects)
			{
				// Evaluating a project means determining the values of properties, the
				// presence of nodes, etc...
				// Properties of a project (for example $(Configuration)) are written
				// back to the environment, but we don't want to one project's enviroment
				// to interfere with the next one, thus we create one environment for each
				// project.
				var projectEnvironment = new BuildEnvironment(environment);
				Project evaluated = _expressionEngine.Evaluate(project, projectEnvironment);
				evaluatedProjects.Add(evaluated, projectEnvironment);
			}
			return evaluatedProjects;
		}
	}
}