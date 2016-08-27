using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BusinessLogic
{
	[TestFixture]
	public sealed class PathTest
	{
		[Test]
		public void TestGetDirectoryWithoutRoot()
		{
			Path.GetDirectoryWithoutRoot(@"C:\Program Files\MSBuild\foo.txt", Slash.Include).Should().Be(@"Program Files\MSBuild\");
			Path.GetDirectoryWithoutRoot(@"C:\Program Files\MSBuild\foo.txt", Slash.Exclude).Should().Be(@"Program Files\MSBuild");

			Path.GetDirectoryWithoutRoot(@"C:\Program Files\MSBuild\", Slash.Include).Should().Be(@"Program Files\MSBuild\");
			Path.GetDirectoryWithoutRoot(@"C:\Program Files\MSBuild\", Slash.Exclude).Should().Be(@"Program Files\MSBuild");
		}
	}
}