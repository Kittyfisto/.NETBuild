using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.DomainModel
{
	[TestFixture]
	public sealed class ProjectItemTest
	{
		[Test]
		public void TestEquality1()
		{
			var item = new ProjectItem("Reference", "mscorlib");
			item.Equals(item).Should().BeTrue();
			item.Equals(new ProjectItem("Reference", "mscorlib")).Should().BeTrue();
		}

		[Test]
		public void TestEquality2()
		{
			var item = new ProjectItem("Reference", "mscorlib", metadata: new List<Metadata>());
			item.Equals(new ProjectItem("Reference", "mscorlib")).Should().BeTrue("Because both items don't contain any metadata");
		}

		[Test]
		public void TestEquality3()
		{
			var item = new ProjectItem("Reference", "mscorlib", metadata: new List<Metadata>
				{
					new Metadata("HintPath", @"..\packages\log4net.2.0.3\lib\net40-full\log4net.dll")
				});
			item.Equals(new ProjectItem("Reference", "mscorlib")).Should().BeFalse();
			item.Equals(new ProjectItem("Reference", "mscorlib", metadata: new List<Metadata>
				{
					new Metadata("HintPath", null)
				})).Should().BeFalse();
			item.Equals(new ProjectItem("Reference", "mscorlib", metadata: new List<Metadata>
				{
					new Metadata("HintPath",  @"packages\log4net.2.0.3\lib\net40-full\log4net.dll")
				})).Should().BeFalse();
		}
	}
}