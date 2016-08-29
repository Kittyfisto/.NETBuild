using System;
using System.Collections.Generic;
using Build.BuildEngine.Tasks;
using Build.BuildEngine.Tasks.Compilers;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	/// <summary>
	/// </summary>
	public sealed class ProjectBuilder
	{
		private readonly ILogger _logger;
		private readonly string _target;
		private readonly BuildEnvironment _environment;
		private readonly AssemblyResolver _resolver;
		private readonly CSharpProject _project;
		private readonly string _tempOutputPath;
		private readonly BuildEnvironment _compileEnvironment;

		public ProjectBuilder(ILogger logger,
		                      AssemblyResolver resolver,
		                      CSharpProject project,
		                      BuildEnvironment environment,
		                      string target)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");
			if (target == null)
				throw new ArgumentNullException("target");

			// TODO: Support different compilers
			_resolver = resolver;
			_project = project;
			_environment = environment;
			_logger = logger;
			_target = target;

			var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
			_tempOutputPath = Path.Combine(projectDirectory,
			                               "obj",
			                               _environment[Properties.Configuration]);

			// Compilation shall be performed into a project exclusive temporary folder
			_compileEnvironment = new BuildEnvironment(_environment, name: "Compiler Environment");
			_compileEnvironment[Properties.OutputPath] = _tempOutputPath;
		}

		public void Run()
		{
			_logger.WriteLine(Verbosity.Quiet, "------ {0} started: Project: {1}, Configuration: {2} {3} ------",
			                  _target,
			                  _environment[Properties.MSBuildProjectName],
			                  _environment[Properties.Configuration],
			                  _environment[Properties.PlatformTarget]
				);

			DateTime started = DateTime.Now;
			_logger.WriteLine(Verbosity.Normal, "Build started {0}.", started);

			switch (_target)
			{
				case Targets.Clean:
					Clean();
					break;

				case Targets.Build:
					Build();
					break;

				default:
					throw new ArgumentException(string.Format("Unknown build target: {0}", _target));
			}

			_logger.WriteLine(Verbosity.Normal, "{0} succeeded.", _target);
			TimeSpan elapsed = DateTime.Now - started;
			_logger.WriteLine(Verbosity.Minimal, "Time Elapsed {0}", elapsed);
		}

		private void Clean()
		{
			var compiler = CreateCompiler(_compileEnvironment);
			var intermediateFiles = new List<string>
				{
					compiler.OutputFilePath
				};
			intermediateFiles.AddRange(compiler.AdditionalOutputFiles);
			var outputFiles = new List<string>();
			foreach (var file in intermediateFiles)
			{
				var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
				var outputPath = Path.MakeAbsolute(projectDirectory, _environment[Properties.OutputPath]);
				var outputFile = Path.Combine(outputPath, Path.GetFilename(file));
				outputFiles.Add(outputFile);
			}

			foreach (var file in intermediateFiles)
			{
				DeleteFile(file);
			}
			foreach (var file in outputFiles)
			{
				DeleteFile(file);
			}
		}

		private void DeleteFile(string fileName)
		{
			var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
			var task = new DeleteFile(_logger, projectDirectory, fileName);
			task.Run();
		}

		private void Build()
		{
			string assembly;
			IEnumerable<string> additionalFiles;
			Compile(out assembly, out additionalFiles);
			CopyToOutputPath(assembly, additionalFiles);
			CopyDependencies();
		}
			
		private void Compile(out string outputFile, out IEnumerable<string> additionalFiles)
		{
			
			var compiler = CreateCompiler(_compileEnvironment);
			compiler.Run();
			outputFile = compiler.OutputFilePath;
			additionalFiles = compiler.AdditionalOutputFiles;
		}

		private void CopyToOutputPath(string outputFile, IEnumerable<string> additionalOutputFiles)
		{
			var destinationFile = CopyToOutputPath(outputFile);
			var projectName = _environment[Properties.MSBuildProjectName];
			_logger.WriteLine(Verbosity.Minimal, "  {0} -> {1}", projectName, destinationFile);

			foreach (var file in additionalOutputFiles)
			{
				CopyToOutputPath(file);
			}
		}

		private string CopyToOutputPath(string inputFilePath)
		{
			var projectDirectory = _environment[Properties.MSBuildProjectDirectory];
			var outputPath = Path.MakeAbsolute(projectDirectory, _environment[Properties.OutputPath]);
			var fileName = Path.GetFilename(inputFilePath);
			var outputFilePath = Path.Combine(outputPath, fileName);
			var task = new CopyFile(_logger, projectDirectory, inputFilePath, outputFilePath, Copy.IfNewer);
			task.Run();
			return outputFilePath;
		}

		private void CopyDependencies()
		{
			
		}

		private IProjectCompiler CreateCompiler(BuildEnvironment environment)
		{
			var compiler = new CSharpProjectCompiler(_resolver, _logger, _project, environment);
			return compiler;
		}
	}
}