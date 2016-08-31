using System;
using System.Collections.Generic;
using System.IO;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.Parser;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.TaskEngine
{
	[TestFixture]
	public sealed class TaskEngineTest
	{
		private Build.ExpressionEngine.ExpressionEngine _expressionEngine;
		private CSharpProjectParser _parser;
		private Mock<ILogger> _logger;
		private List<string> _messages;
		private Mock<IFileSystem> _fileSystem;
		private List<KeyValuePair<string, string>> _copies;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_fileSystem = new Mock<IFileSystem>();
			_copies = new List<KeyValuePair<string, string>>();
			_fileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()))
					   .Callback(
						   (string source, string destination) => _copies.Add(new KeyValuePair<string, string>(source, destination)));

			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine(_fileSystem.Object);

			_parser = CSharpProjectParser.Instance;
			_logger = new Mock<ILogger>();
			_messages = new List<string>();
			_logger.Setup(x => x.WriteLine(It.IsAny<Verbosity>(), It.IsAny<string>(), It.IsAny<object[]>()))
				  .Callback((Verbosity unused, string format, object[] parameters) =>
					  {
						  var message = string.Format(format, parameters);
						  _messages.Add(message);
						  Console.WriteLine(message);
					  });
		}

		[SetUp]
		public void SetUp()
		{
			_messages.Clear();
		}

		[Test]
		public void TestExecuteMessage()
		{
			var project = new Project
				{
					Targets =
						{
							new Target
								{
									Name = "Build",
									Children =
										{
											new Message
												{
													Text = "Hello World!"
												}
										}
								}
						}
				};
			var engine = Create(project, "Build");

			engine.Run();
			_messages.Should().Equal(new object[]
				{
					"Build:",
					"  Hello World!"
				});
		}

		[Test]
		public void TestCopyAppConfigFile()
		{
			var project = _parser.Parse(@"Microsoft\common.props");
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{Properties.AssemblyName, "Foo"},
							{Properties.OutputPath, @"bin\Debug"},
							{Properties.MSBuildProjectDirectory, Directory.GetCurrentDirectory()},
							{Properties.OutputType, "Exe"},
						}
				};
			_fileSystem.Setup(x => x.Exists(It.Is<string>(y => y == "app.config"))).Returns(true);
			var engine = Create(project, "CopyAppConfigFile", environment);
			engine.Run();

			var dir = Directory.GetCurrentDirectory();
			_copies.Count.Should().Be(1);
			_copies[0].Key.Should().Be(Path.Combine(dir, "app.config"));
			_copies[0].Value.Should().Be(Path.Combine(dir, @"bin\Debug\Foo.exe.config"));
		}

		private Build.TaskEngine.TaskEngine Create(Project project, string target, BuildEnvironment environment = null)
		{
			var engine = new Build.TaskEngine.TaskEngine(_expressionEngine,
			                                             _fileSystem.Object,
			                                             project,
			                                             target,
			                                             environment ?? new BuildEnvironment(),
			                                             _logger.Object);
			return engine;
		}
	}
}