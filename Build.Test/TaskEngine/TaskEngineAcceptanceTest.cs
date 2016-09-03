using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.TaskEngine
{
	[TestFixture]
	public sealed class TaskEngineAcceptanceTest
	{
		private IFileSystem _fileSystem;
		private Build.ExpressionEngine.ExpressionEngine _expressionEngine;
		private Build.TaskEngine.TaskEngine _engine;
		private BuildLog _buildLog;
		private BuildEnvironment _environment;

		[SetUp]
		public void SetUp()
		{
			_buildLog = new BuildLog(new Arguments(), new MemoryStream());
			_fileSystem = new FileSystem();
			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine(_fileSystem);
			_engine = new Build.TaskEngine.TaskEngine(_expressionEngine, _fileSystem);
			_environment = new BuildEnvironment();
		}

		#region Hello World

		[Test]
		public void TestBuildHelloWorld()
		{
			var fileName = TestPath.Get(@"TestData\CSharp\HelloWorld\HelloWorld.csproj");
			var project = ProjectParser.Instance.Parse(fileName);

			Clean(@"TestData\CSharp\HelloWorld\bin\Debug\");
			Clean(@"TestData\CSharp\HelloWorld\obj\Debug\");

			_engine.Run(project, Targets.Build, _environment, _buildLog.CreateLogger());

			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe").Should().BeTrue();
			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.pdb").Should().BeTrue();
			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe.config").Should().BeTrue();

			string output;
			var exitCode = Run(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe", out output);
			exitCode.Should().Be(0);
			output.Should().Be("Hello World!");
		}

		#endregion

		#region EmbeddedResource

		[Test]
		public void TestBuildEmbeddedResource()
		{
			var fileName = TestPath.Get(@"TestData\CSharp\EmbeddedResource\EmbeddedResource.csproj");
			var project = ProjectParser.Instance.Parse(fileName);

			Clean(@"TestData\CSharp\EmbeddedResource\bin\Debug\");
			_engine.Run(project, Targets.Build, _environment, _buildLog.CreateLogger());

			var filename = TestPath.Get(@"TestData\CSharp\EmbeddedResource\bin\Debug\EmbeddedResource.dll");
			var assembly = Assembly.LoadFile(filename);
			var resources = assembly.GetManifestResourceNames();
			resources.Should().BeEquivalentTo(new[]
				{
					"EmbeddedResource.HelloWorld.txt",
					"EmbeddedResource.SomeFolder.SomeDataFile.xml",
					"EmbeddedResource.SomeFolder.Some File With Spaces.xml"
				});

			ReadResource(assembly, "EmbeddedResource.HelloWorld.txt").Should().Be("Hello World!");
			ReadResource(assembly, "EmbeddedResource.SomeFolder.SomeDataFile.xml").Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\" ?> ");
			ReadResource(assembly, "EmbeddedResource.SomeFolder.Some File With Spaces.xml").Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\" ?> \r\n<importantdata value=\"42\" />");
		}

		#endregion

		private string ReadResource(Assembly assembly, string resourceName)
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			stream.Should().NotBeNull();

			var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		private int Run(string relativeFileName, out string output)
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
		private bool FileExists(string relativeFilePath)
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