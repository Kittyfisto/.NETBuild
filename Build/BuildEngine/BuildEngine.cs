using System;
using System.Collections.Generic;
using System.Data;
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
		private readonly IFileParser<Solution> _solutionParser;
		private readonly IFileParser<CSharpProject> _projectParser;
		private readonly ExpressionEngine.ExpressionEngine _expressionEngine;
		private readonly string _inputFile;
		private readonly BuildEnvironment _environment;

		public BuildEngine(Arguments arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			_expressionEngine = new ExpressionEngine.ExpressionEngine();
			_solutionParser = new SolutionParser();
			_projectParser = CSharpProjectParser.Instance;
			_inputFile = arguments.InputFile;
			_environment = new BuildEnvironment();
			foreach (var property in arguments.Properties)
			{
				_environment.Add(property.Name, property.Value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Run()
		{
		}

		/// <summary>
		///     Builds all projects using the given environment.
		/// </summary>
		public void Build()
		{
			// Building is done in a few simple steps:

			// #1: Parse all relevant .csproj files into memory
			var projects = Parse();

			// #2: Evaluate these projects using the given environment (dependencies can be attached to conditions)
			// TODO: What do we do when we have conditions that require the presence of files that are from a previous step's output?
			var evaluatedProjects = Evaluate(projects);

			// #3: Build a dependency graph that tells us the relationship between projects
			// TODO: We REALLY want a much more fine grained relation ship graph that includes particular files
			var dependencyGraph = CreateDependencyGraph(evaluatedProjects);

			// #4: Build all projects
		}

		private List<CSharpProject> Parse()
		{
			throw new NoNullAllowedException();
			/*
			var projects = new List<CSharpProject>(_inputFile.Length);
			foreach (var filePath in _inputFile)
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
			return projects;*/
		}

		private Dictionary<CSharpProject, BuildEnvironment> Evaluate(List<CSharpProject> projects)
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
				var projectEnvironment = new BuildEnvironment(_environment);
				var evaluated = _expressionEngine.Evaluate(project, projectEnvironment);
				evaluatedProjects.Add(evaluated, projectEnvironment);
			}
			return evaluatedProjects;
		}

		private ProjectDependencyGraph CreateDependencyGraph(IReadOnlyDictionary<CSharpProject, BuildEnvironment> projects)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			
		}
	}
}