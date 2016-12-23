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
	public sealed class StringLiteralTest
	{
		[Test]
		[Description("Verifies that IFileSystem.CreateProjectItem is used to create item lists")]
		public void TestToItemList1()
		{
			var literal = new StringLiteral("foo.txt");
			var fileSystem = new Mock<IFileSystem>();
			var items = new List<ProjectItem>();
			literal.ToItemList(fileSystem.Object, new BuildEnvironment(), items);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
			                                           It.Is<string>(y => y == "foo.txt"),
			                                           It.Is<string>(y => y == "foo.txt"),
			                                           It.IsAny<BuildEnvironment>()), Times.Once);
		}

		[Test]
		[Description("Verifies that an empty literal evaluates to an empty list")]
		public void TestToItemList2()
		{
			var literal = new StringLiteral("");
			var fileSystem = new Mock<IFileSystem>();
			var items = new List<ProjectItem>();
			literal.ToItemList(fileSystem.Object, new BuildEnvironment(), items);
			items.Should().BeEmpty();
			fileSystem.Verify(x => x.CreateProjectItem(It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<BuildEnvironment>()), Times.Never);
		}

		[Test]
		[Description("Verifies that a literal can evaluate to multiple items if its value references multiple files")]
		public void TestToItemList3()
		{
			var literal = new StringLiteral("a.txt;b.bmp");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			var items = new List<ProjectItem>();
			literal.ToItemList(fileSystem.Object, environment, items);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
													   It.Is<string>(y => y == "a.txt"),
													   It.Is<string>(y => y == "a.txt;b.bmp"),
													   It.IsAny<BuildEnvironment>()), Times.Once);
			fileSystem.Verify(x => x.CreateProjectItem(It.Is<string>(y => y == "None"),
													   It.Is<string>(y => y == "b.bmp"),
													   It.Is<string>(y => y == "a.txt;b.bmp"),
													   It.IsAny<BuildEnvironment>()), Times.Once);
			items.Count.Should().Be(2);
		}
	}
}
