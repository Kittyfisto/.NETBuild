using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class ItemListProjectionTest
	{
		private Mock<IFileSystem> _fileSystem;
		private BuildEnvironment _environment;
		private ProjectItem _item;

		[SetUp]
		public void SetUp()
		{
			_fileSystem = new Mock<IFileSystem>();
			_environment = new BuildEnvironment();
			_item = new ProjectItem();
		}

		[Test]
		public void TestToString1()
		{
			new ItemListProjection("Foo", new StringLiteral("Bar"))
				.ToString().Should().Be("@(Foo -> 'Bar')");
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.ToString().Should().Be("@(Foo -> '%(Filename)')");
			new ItemListProjection("Foo", new MetadataReference("Filename"), new MetadataReference("Extension"))
				.ToString().Should().Be("@(Foo -> '%(Filename)%(Extension)')");
		}

		[Test]
		public void TestToString2()
		{
			new ItemListProjection("Foo", new StringLiteral("Bar"))
				.ToString(_fileSystem.Object, _environment, _item).Should().Be("Bar");
		}

		[Test]
		public void TestToString3()
		{
			_item["Filename"] = "data.txt";
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.ToString(_fileSystem.Object, _environment, _item).Should().Be("data.txt");
		}

		[Test]
		public void TestToString4()
		{
			_environment.Properties["OutputPath"] = @"bin\debug";
			_item["Filename"] = "data";
			_item["Extension"] = ".txt";
			new ItemListProjection("Foo", new VariableReference("OutputPath"), new StringLiteral(@"\"), new MetadataReference("Filename"), new MetadataReference("Extension"))
				.ToString(_fileSystem.Object, _environment, _item).Should().Be(@"bin\debug\data.txt");
		}

		[Test]
		public void TestEquals1()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(null).Should().BeFalse();
		}

		[Test]
		public void TestEquals2()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(new MetadataReference("Bar")).Should().BeFalse();
		}

		[Test]
		public void TestEquals3()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(new ItemListProjection("Foo")).Should().BeFalse();
		}

		[Test]
		public void TestEquals4()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(new ItemListProjection("BAR", new MetadataReference("Filename"))).Should().BeFalse();
		}

		[Test]
		public void TestEquals5()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(new ItemListProjection("Foo", new VariableReference("Filename"))).Should().BeFalse();
		}

		[Test]
		public void TestEquals6()
		{
			new ItemListProjection("Foo", new MetadataReference("Filename"))
				.Equals(new ItemListProjection("Foo", new MetadataReference("Filename"))).Should().BeTrue();
		}

		[Test]
		public void TestToItemList1()
		{
			var projection = new ItemListProjection("Foo", new VariableReference("OutputPath"), new StringLiteral(@"\"),
			                                        new MetadataReference("Filename"), new MetadataReference("Extension"));

			var items = new List<ProjectItem>();
			projection.ToItemList(_fileSystem.Object, _environment, items);
			
		}
	}
}