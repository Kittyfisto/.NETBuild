using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class BuildEnvironmentTest
	{
		[Test]
		[Description("Verifies that an environment represents the items of its parent if available")]
		public void TestItems1()
		{
			var parent = new BuildEnvironment();
			var item = new ProjectItem {Include = "foo.txt"};
			parent.Items.Add(item);
			var env = new BuildEnvironment(parent);
			env.Items.Should().Equal(new object[] {item});
		}

		[Test]
		[Description("Verifies that an environment represents the items of both itself and the parent if available")]
		public void TestItems2()
		{
			var parent = new BuildEnvironment();
			var parentItem = new ProjectItem {Include = "Program.cs"};
			parent.Items.Add(parentItem);
			var env = new BuildEnvironment(parent);
			var item = new ProjectItem{Include = "data.xml"};
			env.Items.Add(item);
			env.Items.Should().BeEquivalentTo(new object[] { parentItem, item });
		}

		[Test]
		[Description("Verifies that an environment does not represent those items of a parent that are already overwritten in the child")]
		public void TestItems3()
		{
			var parent = new BuildEnvironment();
			var parentItem1 = new ProjectItem { Include = "Program.cs" };
			var parentItem2 = new ProjectItem { Include = "Application.cs" };
			parent.Items.AddRange(parentItem1, parentItem2);
			var env = new BuildEnvironment(parent);
			var overwrittenItem = new ProjectItem { Include = "Program.cs" };
			overwrittenItem["SomeImportantValue"] = "42";

			env.Items.Add(overwrittenItem);
			env.Items.Should().BeEquivalentTo(new object[] { parentItem2, overwrittenItem });
		}
	}
}