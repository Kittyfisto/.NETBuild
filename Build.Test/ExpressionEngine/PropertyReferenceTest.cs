using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Build.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class PropertyReferenceTest
	{
		[Test]
		[Description("Verifies that IFileSystem.CreateProjectItem is used to create item lists")]
		public void TestToItemList1()
		{
			var reference = new PropertyReference("Foo");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "a.txt";
			var items = new List<ProjectItem>();
			reference.ToItemList(fileSystem.Object, environment, items);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
													   It.Is<string>(y => y == "a.txt"),
													   It.Is<string>(y => y == "$(Foo)"),
													   It.IsAny<BuildEnvironment>()), Times.Once);
		}

		[Test]
		[Description("Verifies that an unset property evaluates to an empty item list")]
		public void TestToItemList2()
		{
			var reference = new PropertyReference("Foo");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			var items = new List<ProjectItem>();
			reference.ToItemList(fileSystem.Object, environment, items);
			items.Should().BeEmpty();
			fileSystem.Verify(x => x.CreateProjectItem(It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<BuildEnvironment>()), Times.Never);
		}

		[Test]
		[Description("Verifies that a property can evaluate to multiple items if its content references multiple files")]
		public void TestToItemList3()
		{
			var reference = new PropertyReference("Foo");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "a.txt;b.bmp";
			var items = new List<ProjectItem>();
			reference.ToItemList(fileSystem.Object, environment, items);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
													   It.Is<string>(y => y == "a.txt"),
													   It.Is<string>(y => y == "$(Foo)"),
													   It.IsAny<BuildEnvironment>()), Times.Once);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
													   It.Is<string>(y => y == "b.bmp"),
													   It.Is<string>(y => y == "$(Foo)"),
													   It.IsAny<BuildEnvironment>()), Times.Once);

			items.Count.Should().Be(2);
		}
	}
}