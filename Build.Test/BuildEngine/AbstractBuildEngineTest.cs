using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public abstract class AbstractBuildEngineTest
	{
		protected abstract string[] ProjectDirectories { get; }

		protected abstract string[] ExpectedOutputFiles { get; }

		protected abstract string ProjectFilePath { get; }

		[Test]
		public void TestBuild()
		{
			Clean();

			var arguments = new Arguments
				{
					InputFile = TestPath.Get(Path.Combine(@"TestData\CSharp", ProjectFilePath)),
					Verbosity = Verbosity.Minimal
				};
			using (var engine = new Build.BuildEngine.BuildEngine(arguments))
			{
				new Action(() => engine.Execute())
					.ShouldNotThrow("Because building this project should succeed");

				var log = engine.Log;
				log.Warnings.Should().BeEmpty("Because no warnings should've been encountered during the build process");
				log.Errors.Should().BeEmpty("Because no errors shoud've been encountered during the build process");
			}

			foreach (var file in ExpectedOutputFiles)
			{
				var path = TestPath.Get(Path.Combine(@"TestData\CSharp", file));
				File.Exists(path).Should().BeTrue("because this file should've been created during the build process");
			}

			PostBuildChecks();
		}

		protected abstract void PostBuildChecks();

		private void Clean()
		{
			foreach (var projectDirectory in ProjectDirectories)
			{
				var directory = TestPath.Get(Path.Combine(@"TestData\CSharp", projectDirectory));

				Clean(Path.Combine(directory, "bin", "debug"));
				Clean(Path.Combine(directory, "bin", "release"));
				Clean(Path.Combine(directory, "obj", "debug"));
				Clean(Path.Combine(directory, "obj", "release"));
			}
		}

		protected string ReadResource(Assembly assembly, string resourceName)
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			stream.Should().NotBeNull();

			var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		protected int Run(string relativeFileName, out string output)
		{
			var fullPath = TestPath.Get(relativeFileName);
			var workingDirectory = Path.GetDirectory(fullPath);

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
			var path = TestPath.Get(relativeFilePath);
			return File.Exists(path);
		}

		private void Clean(string relativeFolderName)
		{
			if (relativeFolderName.Contains(".."))
				throw new Exception();

			var folderPath = Path.Normalize(TestPath.Get(relativeFolderName));
			if (!Directory.Exists(folderPath))
				return;

			Console.WriteLine("Cleaning '{0}'", folderPath);
			var files = Directory.EnumerateFiles(folderPath, "*.*");
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}
	}
}