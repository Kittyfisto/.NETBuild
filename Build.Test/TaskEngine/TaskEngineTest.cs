using System.Collections.Generic;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.TaskEngine
{
	[TestFixture]
	public sealed class TaskEngineTest
	{
		private Build.ExpressionEngine.ExpressionEngine _expressionEngine;
		private Mock<ILogger> _logger;
		private List<string> _messages;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine();

			_logger = new Mock<ILogger>();
			_messages = new List<string>();
			_logger.Setup(x => x.WriteLine(It.IsAny<Verbosity>(), It.IsAny<string>(), It.IsAny<object[]>()))
				  .Callback((Verbosity unused, string format, object[] parameters) => _messages.Add(string.Format(format, parameters)));
		}

		[SetUp]
		public void SetUp()
		{
			_messages.Clear();
		}

		[Test]
		public void TestExecuteMessage()
		{
			var project = new Project("foo")
				{
					Targets =
						{
							new Target
								{
									Name = "Build",
									Tasks =
										{
											new Message
												{
													Text = "Hello World!"
												}
										}
								}
						}
				};
			var engine = new Build.TaskEngine.TaskEngine(_expressionEngine,
			                            project,
			                            "Build",
			                            new BuildEnvironment(),
			                            _logger.Object);

			engine.Run();
			_messages.Should().Equal(new object[]
				{
					"Build:",
					"  Hello World!"
				});
		}
	}
}