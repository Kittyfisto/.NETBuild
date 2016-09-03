using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public abstract class AbstractBuildEngineTest
	{
		[SetUp]
		public void SetUp()
		{
			CleanOutputPath();
		}

		protected abstract string[] ProjectDirectories { get; }

		protected abstract string[] ExpectedOutputFiles { get; }

		protected abstract string ProjectFilePath { get; }

		private void Build()
		{
			var arguments = new Arguments
				{
					InputFile = TestPath.Get(Path.Combine(@"TestData\CSharp", ProjectFilePath)),
					Targets = {Targets.Build},
					Verbosity = Verbosity.Minimal
				};
			using (var engine = new Build.BuildEngine.BuildEngine(arguments))
			{
				new Action(() => engine.Execute())
					.ShouldNotThrow("Because building this project should succeed");

				BuildLog log = engine.Log;
				log.Warnings.Should().BeEmpty("Because no warnings should've been encountered during the build process");
				log.Errors.Should().BeEmpty("Because no errors shoud've been encountered during the build process");
			}
		}

		protected abstract void PostBuildChecks();

		private void CleanOutputPath()
		{
			foreach (string projectDirectory in ProjectDirectories)
			{
				string directory = TestPath.Get(Path.Combine(@"TestData\CSharp", projectDirectory));

				Clean(Path.Combine(directory, "bin", "debug"));
				Clean(Path.Combine(directory, "bin", "release"));
				Clean(Path.Combine(directory, "obj", "debug"));
				Clean(Path.Combine(directory, "obj", "release"));
			}
		}

		protected string ReadResource(Assembly assembly, string resourceName)
		{
			Stream stream = assembly.GetManifestResourceStream(resourceName);
			stream.Should().NotBeNull();

			var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		protected int Run(string relativeFileName, out string output)
		{
			string fullPath = TestPath.Get(relativeFileName);
			string workingDirectory = Path.GetDirectory(fullPath);

			Console.WriteLine("Executing '{0}'", fullPath);

			int exitCode = ProcessEx.Run(fullPath,
			                             workingDirectory,
			                             new ArgumentBuilder(),
			                             out output);
			return exitCode;
		}

		[Pure]
		protected bool FileExists(string relativeFilePath)
		{
			string path = TestPath.Get(relativeFilePath);
			return File.Exists(path);
		}

		private void Clean(string relativeFolderName)
		{
			if (relativeFolderName.Contains(".."))
				throw new Exception();

			string folderPath = Path.Normalize(TestPath.Get(relativeFolderName));
			if (!Directory.Exists(folderPath))
				return;

			Console.WriteLine("Cleaning '{0}'", folderPath);
			IEnumerable<string> files = Directory.EnumerateFiles(folderPath, "*.*");
			foreach (string file in files)
			{
				File.Delete(file);
			}
		}

		private void Clean()
		{
			var arguments = new Arguments
				{
					InputFile = TestPath.Get(Path.Combine(@"TestData\CSharp", ProjectFilePath)),
					Targets = {Targets.Clean},
					Verbosity = Verbosity.Normal
				};
			using (var engine = new Build.BuildEngine.BuildEngine(arguments))
			{
				new Action(() => engine.Execute())
					.ShouldNotThrow("Because cleaning this project should succeed");
			}
		}

		[Test]
		public void TestBuild()
		{
			Build();

			foreach (string file in ExpectedOutputFiles)
			{
				string path = TestPath.Get(Path.Combine(@"TestData\CSharp", file));
				File.Exists(path).Should().BeTrue(
					"because the file '{0}' should've been created during the build process",
					path);
			}

			PostBuildChecks();

			Clean();

			foreach (string file in ExpectedOutputFiles)
			{
				string path = TestPath.Get(Path.Combine(@"TestData\CSharp", file));
				File.Exists(path).Should().BeFalse(
					"because the file '{0}' should've been deleted during the clean process",
					path);
			}
		}
	}
}