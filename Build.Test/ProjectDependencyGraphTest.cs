using System;
using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test
{
	[TestFixture]
	public sealed class ProjectDependencyGraphTest
	{
		private ProjectDependencyGraph _graph;

		[SetUp]
		public void SetUp()
		{
			_graph = new ProjectDependencyGraph();
		}

		[Test]
		public void TestSucceed1()
		{
			var p = new Project { Filename = @"C:\1" };
			_graph.Add(p, new BuildEnvironment());

			Project next;
			BuildEnvironment unused;
			_graph.TryGetNextProject(out next, out unused).Should().BeTrue("because the only added project has not dependencies and thus should be ready to be built");

			_graph.IsFinished.Should().BeFalse("Because the only project has neither succeeded, nor failed (yet)");
			_graph.FinishedEvent.Wait(TimeSpan.Zero).Should().BeFalse();


			_graph.MarkAsSuccess(p);
			_graph.IsFinished.Should().BeTrue("Because we've succeeded building the only project");
			_graph.FinishedEvent.Wait(TimeSpan.Zero).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that once a project is marked as having failed, then all of its dependend projects are removed from the todo list")]
		public void TestFailure1()
		{
			var p1 = new Project {Filename = @"C:\1"};
			var p2 = new Project {Filename = @"C:\2"};

			_graph.Add(p1, new BuildEnvironment());
			_graph.Add(p2, new BuildEnvironment());
			_graph.AddDependency(p2, p1);

			Project next;
			BuildEnvironment unused;
			_graph.TryGetNextProject(out next, out unused).Should().BeTrue();
			next.Should().Be(p1);

			_graph.MarkAsFailed(p1);
			_graph.FailedCount.Should().Be(1);
			_graph.TryGetNextProject(out next, out unused)
			      .Should()
			      .BeFalse("because p2 depends on p1 and because the latter has failed, the former may not be build next");

			_graph.FinishedEvent.Wait(TimeSpan.Zero).Should().BeTrue("Because no more projects can be build");
		}
	}
}