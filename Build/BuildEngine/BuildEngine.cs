using System;
using System.Collections.Generic;
using Build.DomainModel;
using Build.DomainModel.MSBuild;
using Build.Parser;

namespace Build.BuildEngine
{
	/// <summary>
	///     Responsible for building a list of (Visual Studio) solutions and/or projects.
	/// </summary>
	public sealed class BuildEngine
	{
		private readonly IFileParser<Solution> _solutionParser;
		private readonly IFileParser<CSharpProject> _projectParser;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly string[] _filePaths;

		public BuildEngine(params string[] filePaths)
		{
			if (filePaths == null)
				throw new ArgumentNullException("filePaths");

			_expressionEngine = new ExpressionEngine.ExpressionEngine();
			_solutionParser = new SolutionParser();
			_projectParser = CSharpProjectParser.Instance;
			_filePaths = filePaths;
		}

		/// <summary>
		///     Builds all projects using the given environment.
		/// </summary>
		public void Build(BuildEnvironment environment)
		{
			// Building is done in a few simple steps:

			// #1: Parse all relevant .csproj files into memory
			var projects = Parse();

			// #2: Evaluate these projects using the given environment (dependencies can be attached to conditions)
			// TODO: What do we do when we have conditions that require the presence of files that are from a previous step's output?
			var evaluatedProjects = Evaluate(projects, environment);

			// #3: Build a dependency graph that tells us the relationship between projects
			// TODO: We REALLY want a much more fine grained relation ship graph that includes particular files
			var dependencyGraph = CreateDependencyGraph(evaluatedProjects);

			// #4: Build all projects
		}

		private List<CSharpProject> Parse()
		{
			var projects = new List<CSharpProject>(_filePaths.Length);
			foreach (var filePath in _filePaths)
			{
				var extension = Path.GetExtension(filePath);
				switch (extension)
				{
					case "sln":
						var solution = _solutionParser.Parse(filePath);
						projects.AddRange(solution.Projects);
						break;

					case "csproj":
						var project = _projectParser.Parse(filePath);
						projects.Add(project);
						break;

					default:
						throw new Exception(string.Format("Unsupported file-type: {0}", extension));
				}
			}
			return projects;
		}

		private Dictionary<CSharpProject, BuildEnvironment> Evaluate(List<CSharpProject> projects, BuildEnvironment environment)
		{
			var evaluatedProjects = new Dictionary<CSharpProject, BuildEnvironment>(projects.Count);
			foreach (var project in projects)
			{
				// Evaluating a project means determining the values of properties, the
				// presence of nodes, etc...
				// Properties of a project (for example $(Configuration)) are written
				// back to the environment, but we don't want to one project's enviroment
				// to interfere with the next one, thus we create one environment for each
				// project.
				var projectEnvironment = new BuildEnvironment(environment);
				var evaluated = _expressionEngine.Evaluate(project, projectEnvironment);
				evaluatedProjects.Add(evaluated, projectEnvironment);
			}
			return evaluatedProjects;
		}

		private ProjectDependencyGraph CreateDependencyGraph(IReadOnlyDictionary<CSharpProject, BuildEnvironment> projects)
		{
			throw new NotImplementedException();
		}
	}
}