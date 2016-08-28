using FluentAssertions;
using NUnit.Framework;

namespace Build.Test
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

		[Test]
		public void TestMakeRelative()
		{
			Path.MakeRelative(@"C:\Program Files\", @"C:\Program Files\MSBuild\").Should().Be(@"MSBuild\");
			Path.MakeRelative(@"C:\Program Files\MSBuild\", @"C:\Program Files\MSBuild\bin\debug\foo.exe").Should().Be(@"bin\debug\foo.exe");
		}
	}
}