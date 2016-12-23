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
		[Description("Verifies that an unset property evaluates to an empty item list")]
		public void TestToItemList1()
		{
			var reference = new PropertyReference("Foo");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\";
			var items = new List<ProjectItem>();
			reference.ToItemList(fileSystem.Object, environment, items);
			items.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that a property can evaluate to multiple items if its content references multiple files")]
		public void TestToItemList2()
		{
			var reference = new PropertyReference("Foo");
			var fileSystem = new Mock<IFileSystem>();
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\";
			environment.Properties["Foo"] = "a.txt;b.bmp";
			var items = new List<ProjectItem>();
			reference.ToItemList(fileSystem.Object, environment, items);

			items.Count.Should().Be(2);
			items[0][Metadatas.Filename].Should().Be("a");
			items[0][Metadatas.Extension].Should().Be(".txt");

			items[1][Metadatas.Filename].Should().Be("b");
			items[1][Metadatas.Extension].Should().Be(".bmp");
		}
	}
}