using System;
using System.Collections.Generic;
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
		private ProjectParser _parser;
		private Mock<ILogger> _logger;
		private List<string> _messages;
		private Mock<IFileSystem> _fileSystem;
		private List<KeyValuePair<string, string>> _copies;
		private Build.TaskEngine.TaskEngine _engine;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_fileSystem = new Mock<IFileSystem>();
			_copies = new List<KeyValuePair<string, string>>();
			_fileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
					   .Callback(
						   (string source, string destination, bool unused) => _copies.Add(new KeyValuePair<string, string>(source, destination)));

			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine(_fileSystem.Object);

			_parser = ProjectParser.Instance;
			_logger = new Mock<ILogger>();
			_messages = new List<string>();
			_logger.Setup(x => x.WriteLine(It.IsAny<Verbosity>(), It.IsAny<string>(), It.IsAny<object[]>()))
				  .Callback((Verbosity unused, string format, object[] parameters) =>
					  {
						  var message = string.Format(format, parameters);
						  _messages.Add(message);
						  Console.WriteLine(message);
					  });

			_engine = new Build.TaskEngine.TaskEngine(_expressionEngine,
			                                          _fileSystem.Object);
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
					Filename = "foo",
					Targets =
						{
							new Target
								{
									Name = "SomeMessage",
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

			_engine.Run(project, "SomeMessage", new BuildEnvironment(), _logger.Object);
			_messages.Should().Contain(new object[]
				{
					"SomeMessage:",
					"  Hello World!"
				});
		}
	}
}